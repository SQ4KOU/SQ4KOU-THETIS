using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Collections;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SyntheticBoundNodeFactory
{
	public class MissingPredefinedMember : Exception
	{
		public Diagnostic Diagnostic { get; }

		public MissingPredefinedMember(Diagnostic error)
			: base(error.ToString())
		{
			Diagnostic = error;
		}
	}

	internal readonly struct SyntheticSwitchSection(ImmutableArray<int> values, ImmutableArray<BoundStatement> statements)
	{
		public readonly ImmutableArray<int> Values = values;

		public readonly ImmutableArray<BoundStatement> Statements = statements;
	}

	private NamedTypeSymbol? _currentType;

	private MethodSymbol? _currentFunction;

	private MethodSymbol? _topLevelMethod;

	public CSharpCompilation Compilation => CompilationState.Compilation;

	public SyntaxNode Syntax { get; set; }

	public PEModuleBuilder? ModuleBuilderOpt => CompilationState.ModuleBuilderOpt;

	public BindingDiagnosticBag Diagnostics { get; }

	public InstrumentationState? InstrumentationState { get; }

	public TypeCompilationState CompilationState { get; }

	public NamedTypeSymbol? CurrentType
	{
		get
		{
			return _currentType;
		}
		set
		{
			_currentType = value;
		}
	}

	public MethodSymbol? CurrentFunction
	{
		get
		{
			return _currentFunction;
		}
		set
		{
			_currentFunction = value;
			if ((object)value != null && value.MethodKind != MethodKind.AnonymousFunction && value.MethodKind != MethodKind.LocalFunction)
			{
				_topLevelMethod = value;
				_currentType = value.ContainingType;
			}
		}
	}

	public MethodSymbol? TopLevelMethod
	{
		get
		{
			return _topLevelMethod;
		}
		private set
		{
			_topLevelMethod = value;
		}
	}

	public SyntheticBoundNodeFactory(MethodSymbol topLevelMethod, SyntaxNode node, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics, InstrumentationState? instrumentationState = null)
		: this(topLevelMethod, topLevelMethod.ContainingType, node, compilationState, diagnostics, instrumentationState)
	{
	}

	public SyntheticBoundNodeFactory(MethodSymbol? topLevelMethodOpt, NamedTypeSymbol? currentClassOpt, SyntaxNode node, TypeCompilationState compilationState, BindingDiagnosticBag diagnostics, InstrumentationState? instrumentationState = null)
	{
		CompilationState = compilationState;
		CurrentType = currentClassOpt;
		TopLevelMethod = topLevelMethodOpt;
		CurrentFunction = topLevelMethodOpt;
		Syntax = node;
		Diagnostics = diagnostics;
		InstrumentationState = instrumentationState;
	}

	[Conditional("DEBUG")]
	private void CheckCurrentType()
	{
		_ = CurrentType;
	}

	public void AddNestedType(NamedTypeSymbol nestedType)
	{
		ModuleBuilderOpt.AddSynthesizedDefinition(nestedType.ContainingType, nestedType.GetCciAdapter());
	}

	public void OpenNestedType(NamedTypeSymbol nestedType)
	{
		AddNestedType(nestedType);
		CurrentFunction = null;
		TopLevelMethod = null;
		CurrentType = nestedType;
	}

	public BoundHoistedFieldAccess HoistedField(FieldSymbol field)
	{
		return new BoundHoistedFieldAccess(Syntax, field, field.Type);
	}

	public StateMachineFieldSymbol StateMachineField(TypeWithAnnotations type, string name, bool isPublic = false, bool isThis = false)
	{
		StateMachineFieldSymbol stateMachineFieldSymbol = new StateMachineFieldSymbol(CurrentType, type, name, isPublic, isThis);
		AddField(CurrentType, stateMachineFieldSymbol);
		return stateMachineFieldSymbol;
	}

	public StateMachineFieldSymbol StateMachineField(TypeSymbol type, string name, bool isPublic = false, bool isThis = false)
	{
		StateMachineFieldSymbol stateMachineFieldSymbol = new StateMachineFieldSymbol(CurrentType, TypeWithAnnotations.Create(type), name, isPublic, isThis);
		AddField(CurrentType, stateMachineFieldSymbol);
		return stateMachineFieldSymbol;
	}

	public StateMachineFieldSymbol StateMachineFieldForRegularParameter(TypeSymbol type, string name, ParameterSymbol parameter, bool isPublic)
	{
		StateMachineFieldSymbolForRegularParameter stateMachineFieldSymbolForRegularParameter = new StateMachineFieldSymbolForRegularParameter(CurrentType, TypeWithAnnotations.Create(type), name, parameter, isPublic);
		AddField(CurrentType, stateMachineFieldSymbolForRegularParameter);
		return stateMachineFieldSymbolForRegularParameter;
	}

	public StateMachineFieldSymbol StateMachineField(TypeSymbol type, string name, SynthesizedLocalKind synthesizedKind, int slotIndex)
	{
		StateMachineFieldSymbol stateMachineFieldSymbol = new StateMachineFieldSymbol(CurrentType, type, name, synthesizedKind, slotIndex, isPublic: false);
		AddField(CurrentType, stateMachineFieldSymbol);
		return stateMachineFieldSymbol;
	}

	public StateMachineFieldSymbol StateMachineField(TypeSymbol type, string name, LocalSlotDebugInfo slotDebugInfo, int slotIndex)
	{
		StateMachineFieldSymbol stateMachineFieldSymbol = new StateMachineFieldSymbol(CurrentType, type, name, slotDebugInfo, slotIndex, isPublic: false);
		AddField(CurrentType, stateMachineFieldSymbol);
		return stateMachineFieldSymbol;
	}

	public void AddField(NamedTypeSymbol containingType, FieldSymbol field)
	{
		ModuleBuilderOpt.AddSynthesizedDefinition(containingType, field.GetCciAdapter());
	}

	public GeneratedLabelSymbol GenerateLabel(string prefix)
	{
		return new GeneratedLabelSymbol(prefix);
	}

	public BoundThisReference This()
	{
		return new BoundThisReference(Syntax, CurrentFunction.ThisParameter.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression This(LocalSymbol thisTempOpt)
	{
		if (!(thisTempOpt != null))
		{
			return This();
		}
		return Local(thisTempOpt);
	}

	public BoundBaseReference Base(NamedTypeSymbol baseType)
	{
		return new BoundBaseReference(Syntax, baseType)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundBadExpression BadExpression(TypeSymbol type)
	{
		return new BoundBadExpression(Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, ImmutableArray<BoundExpression>.Empty, type, hasErrors: true);
	}

	public BoundParameter Parameter(ParameterSymbol p)
	{
		return new BoundParameter(Syntax, p, p.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundFieldAccess Field(BoundExpression? receiver, FieldSymbol f)
	{
		return new BoundFieldAccess(Syntax, receiver, f, null, LookupResultKind.Viable, f.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundFieldAccess InstanceField(FieldSymbol f)
	{
		return Field(This(), f);
	}

	public BoundExpression Property(WellKnownMember member)
	{
		return Property(null, member);
	}

	public BoundExpression Property(BoundExpression? receiverOpt, WellKnownMember member)
	{
		PropertySymbol propertySymbol = (PropertySymbol)WellKnownMember(member);
		Binder.ReportUseSite(propertySymbol, Diagnostics, Syntax);
		return Property(receiverOpt, propertySymbol);
	}

	public BoundExpression Property(BoundExpression? receiverOpt, PropertySymbol property)
	{
		MethodSymbol ownOrInheritedGetMethod = property.GetOwnOrInheritedGetMethod();
		return Call(receiverOpt, ownOrInheritedGetMethod);
	}

	public BoundExpression Indexer(BoundExpression? receiverOpt, PropertySymbol property, BoundExpression arg0)
	{
		MethodSymbol ownOrInheritedGetMethod = property.GetOwnOrInheritedGetMethod();
		return Call(receiverOpt, ownOrInheritedGetMethod, arg0);
	}

	public NamedTypeSymbol SpecialType(SpecialType st)
	{
		NamedTypeSymbol specialType = Compilation.GetSpecialType(st);
		Binder.ReportUseSite(specialType, Diagnostics, Syntax);
		return specialType;
	}

	public ArrayTypeSymbol WellKnownArrayType(WellKnownType elementType)
	{
		return Compilation.CreateArrayTypeSymbol(WellKnownType(elementType));
	}

	public NamedTypeSymbol WellKnownType(WellKnownType wt)
	{
		NamedTypeSymbol wellKnownType = Compilation.GetWellKnownType(wt);
		Binder.ReportUseSite(wellKnownType, Diagnostics, Syntax);
		return wellKnownType;
	}

	public Symbol? WellKnownMember(WellKnownMember wm, bool isOptional)
	{
		Symbol wellKnownTypeMember = Binder.GetWellKnownTypeMember(Compilation, wm, Diagnostics, null, Syntax, isOptional: true);
		if ((object)wellKnownTypeMember == null && !isOptional)
		{
			MemberDescriptor descriptor = WellKnownMembers.GetDescriptor(wm);
			throw new MissingPredefinedMember(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, descriptor.DeclaringTypeMetadataName, descriptor.Name), Syntax.Location));
		}
		return wellKnownTypeMember;
	}

	public Symbol WellKnownMember(WellKnownMember wm)
	{
		return WellKnownMember(wm, isOptional: false);
	}

	public MethodSymbol? WellKnownMethod(WellKnownMember wm, bool isOptional)
	{
		return (MethodSymbol)WellKnownMember(wm, isOptional);
	}

	public MethodSymbol WellKnownMethod(WellKnownMember wm)
	{
		return (MethodSymbol)WellKnownMember(wm, isOptional: false);
	}

	public Symbol SpecialMember(SpecialMember sm)
	{
		return SpecialMember(sm, false);
	}

	public Symbol? SpecialMember(SpecialMember sm, bool isOptional = false)
	{
		Symbol specialTypeMember = Compilation.GetSpecialTypeMember(sm);
		if ((object)specialTypeMember == null)
		{
			if (isOptional)
			{
				return null;
			}
			MemberDescriptor descriptor = SpecialMembers.GetDescriptor(sm);
			throw new MissingPredefinedMember(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, descriptor.DeclaringTypeMetadataName, descriptor.Name), Syntax.Location));
		}
		UseSiteInfo<AssemblySymbol> useSiteInfo = specialTypeMember.GetUseSiteInfo();
		if (isOptional)
		{
			DiagnosticInfo? diagnosticInfo = useSiteInfo.DiagnosticInfo;
			if (diagnosticInfo != null && diagnosticInfo.DefaultSeverity == DiagnosticSeverity.Error)
			{
				return null;
			}
		}
		else
		{
			Diagnostics.Add(useSiteInfo, Syntax);
		}
		return specialTypeMember;
	}

	public MethodSymbol SpecialMethod(SpecialMember sm)
	{
		return (MethodSymbol)SpecialMember(sm, false);
	}

	public MethodSymbol? SpecialMethod(SpecialMember sm, bool isOptional)
	{
		return (MethodSymbol)SpecialMember(sm, isOptional);
	}

	public PropertySymbol SpecialProperty(SpecialMember sm)
	{
		return (PropertySymbol)SpecialMember(sm);
	}

	public BoundExpressionStatement Assignment(BoundExpression left, BoundExpression right, bool isRef = false)
	{
		return ExpressionStatement(AssignmentExpression(left, right, isRef));
	}

	public BoundExpressionStatement ExpressionStatement(BoundExpression expr)
	{
		return new BoundExpressionStatement(Syntax, expr)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression AssignmentExpression(BoundExpression left, BoundExpression right, bool isRef = false)
	{
		return AssignmentExpression(Syntax, left, right, isRef, hasErrors: false, wasCompilerGenerated: true);
	}

	public BoundExpression AssignmentExpression(SyntaxNode syntax, BoundExpression left, BoundExpression right, bool isRef = false, bool hasErrors = false, bool wasCompilerGenerated = false)
	{
		BoundAssignmentOperator boundAssignmentOperator = new BoundAssignmentOperator(syntax, left, right, isRef, left.Type, hasErrors)
		{
			WasCompilerGenerated = wasCompilerGenerated
		};
		InstrumentationState? instrumentationState = InstrumentationState;
		bool flag = instrumentationState != null && !instrumentationState.IsSuppressed;
		bool flag2;
		if (flag)
		{
			if (left is BoundLocal boundLocal)
			{
				LocalSymbol localSymbol = boundLocal.LocalSymbol;
				if ((object)localSymbol != null && localSymbol.SynthesizedKind == SynthesizedLocalKind.UserDefined)
				{
					goto IL_005a;
				}
			}
			else if (left is BoundParameter)
			{
				goto IL_005a;
			}
			flag2 = false;
			goto IL_0062;
		}
		goto IL_0065;
		IL_0062:
		flag = flag2;
		goto IL_0065;
		IL_005a:
		flag2 = true;
		goto IL_0062;
		IL_0065:
		if (!flag)
		{
			return boundAssignmentOperator;
		}
		return InstrumentationState.Instrumenter.InstrumentUserDefinedLocalAssignment(boundAssignmentOperator);
	}

	public BoundBlock Block()
	{
		return Block(ImmutableArray<BoundStatement>.Empty);
	}

	public BoundBlock Block(ImmutableArray<BoundStatement> statements)
	{
		return Block(ImmutableArray<LocalSymbol>.Empty, statements);
	}

	public BoundBlock Block(params BoundStatement[] statements)
	{
		return Block(ImmutableArray.Create(statements));
	}

	public BoundBlock Block(ImmutableArray<LocalSymbol> locals, params BoundStatement[] statements)
	{
		return Block(locals, ImmutableArray.Create(statements));
	}

	public BoundBlock Block(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundStatement> statements)
	{
		return new BoundBlock(Syntax, locals, statements)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundBlock Block(ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, params BoundStatement[] statements)
	{
		return Block(locals, localFunctions, ImmutableArray.Create(statements));
	}

	public BoundBlock Block(ImmutableArray<LocalSymbol> locals, ImmutableArray<LocalFunctionSymbol> localFunctions, ImmutableArray<BoundStatement> statements)
	{
		return Block(locals, ImmutableArray<MethodSymbol>.CastUp(localFunctions), statements);
	}

	public BoundBlock Block(ImmutableArray<LocalSymbol> locals, ImmutableArray<MethodSymbol> localFunctions, ImmutableArray<BoundStatement> statements)
	{
		return new BoundBlock(Syntax, locals, localFunctions, hasUnsafeModifier: false, null, statements)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExtractedFinallyBlock ExtractedFinallyBlock(BoundBlock finallyBlock)
	{
		return new BoundExtractedFinallyBlock(Syntax, finallyBlock)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundStatementList StatementList()
	{
		return StatementList(ImmutableArray<BoundStatement>.Empty);
	}

	public BoundStatementList StatementList(ImmutableArray<BoundStatement> statements)
	{
		return new BoundStatementList(Syntax, statements)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundStatementList StatementList(BoundStatement first, BoundStatement second)
	{
		return new BoundStatementList(Syntax, ImmutableArray.Create(first, second))
		{
			WasCompilerGenerated = true
		};
	}

	[return: NotNullIfNotNull("first")]
	[return: NotNullIfNotNull("second")]
	public BoundStatement? Concat(BoundStatement? first, BoundStatement? second)
	{
		if (first != null)
		{
			if (second != null)
			{
				return StatementList(first, second);
			}
			return first;
		}
		return second;
	}

	public BoundBlockInstrumentation CombineInstrumentation(BoundBlockInstrumentation? innerInstrumentation = null, LocalSymbol? local = null, BoundStatement? prologue = null, BoundStatement? epilogue = null)
	{
		if (innerInstrumentation == null)
		{
			return new BoundBlockInstrumentation(Syntax, (local != null) ? ImmutableCollectionsMarshal.AsImmutableArray(new LocalSymbol[1] { local }) : ImmutableArray<LocalSymbol>.Empty, prologue, epilogue);
		}
		return new BoundBlockInstrumentation(innerInstrumentation.Syntax, (local != null) ? innerInstrumentation.Locals.Add(local) : innerInstrumentation.Locals, (prologue != null) ? Concat(prologue, innerInstrumentation.Prologue) : innerInstrumentation.Prologue, (epilogue != null) ? Concat(innerInstrumentation.Epilogue, epilogue) : innerInstrumentation.Epilogue);
	}

	public BoundStatement Instrument(BoundStatement statement, BoundBlockInstrumentation? instrumentation)
	{
		if (instrumentation == null)
		{
			return statement;
		}
		TemporaryArray<BoundStatement> temporaryArray = default(TemporaryArray<BoundStatement>);
		if (instrumentation.Prologue != null)
		{
			temporaryArray.Add(instrumentation.Prologue);
		}
		if (instrumentation.Epilogue != null)
		{
			temporaryArray.Add(Try(Block(statement), ImmutableArray<BoundCatchBlock>.Empty, Block(instrumentation.Epilogue)));
		}
		else
		{
			temporaryArray.Add(statement);
		}
		return Block(instrumentation.Locals, temporaryArray.ToImmutableAndClear());
	}

	public BoundReturnStatement Return(BoundExpression? expression = null)
	{
		if (expression != null)
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			Conversion conversion = Compilation.Conversions.ClassifyConversionFromType(expression.Type, CurrentFunction.ReturnType, isChecked: false, ref useSiteInfo);
			if (conversion.Kind != ConversionKind.Identity)
			{
				expression = BoundConversion.Synthesized(Syntax, expression, conversion, @checked: false, explicitCastInCode: false, null, null, CurrentFunction.ReturnType);
			}
		}
		return new BoundReturnStatement(Syntax, (CurrentFunction.RefKind != RefKind.None) ? RefKind.Ref : RefKind.None, expression, @checked: false)
		{
			WasCompilerGenerated = true
		};
	}

	public void CloseMethod(BoundStatement body)
	{
		if (body.Kind != BoundKind.Block)
		{
			body = Block(body);
		}
		CompilationState.AddSynthesizedMethod(CurrentFunction, body);
		CurrentFunction = null;
	}

	public LocalSymbol SynthesizedLocal(TypeSymbol type, SyntaxNode? syntax = null, bool isPinned = false, bool isKnownToReferToTempIfReferenceType = false, RefKind refKind = RefKind.None, SynthesizedLocalKind kind = SynthesizedLocalKind.LoweringTemp)
	{
		return new SynthesizedLocal(CurrentFunction, TypeWithAnnotations.Create(type), kind, syntax, isPinned, isKnownToReferToTempIfReferenceType, refKind);
	}

	public LocalSymbol InterpolatedStringHandlerLocal(TypeSymbol type, SyntaxNode syntax)
	{
		return new SynthesizedLocal(CurrentFunction, TypeWithAnnotations.Create(type), SynthesizedLocalKind.LoweringTemp, syntax);
	}

	public ParameterSymbol SynthesizedParameter(TypeSymbol type, string name, MethodSymbol? container = null, int ordinal = 0)
	{
		return SynthesizedParameterSymbol.Create(container, TypeWithAnnotations.Create(type), ordinal, RefKind.None, name);
	}

	public BoundBinaryOperator Binary(BinaryOperatorKind kind, TypeSymbol type, BoundExpression left, BoundExpression right)
	{
		return new BoundBinaryOperator(Syntax, kind, null, null, null, LookupResultKind.Viable, left, right, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundAsOperator As(BoundExpression operand, TypeSymbol type)
	{
		return new BoundAsOperator(Syntax, operand, Type(type), null, null, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundIsOperator Is(BoundExpression operand, TypeSymbol type)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		Conversion conversion = Compilation.Conversions.ClassifyBuiltInConversion(operand.Type, type, isChecked: false, ref useSiteInfo);
		return new BoundIsOperator(Syntax, operand, Type(type), conversion.Kind, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundBinaryOperator LogicalAnd(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.LogicalBoolAnd, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator LogicalOr(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.LogicalBoolOr, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator IntEqual(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.IntEqual, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator ObjectEqual(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.ObjectEqual, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundExpression IsNotNullReference(BoundExpression value)
	{
		NamedTypeSymbol namedTypeSymbol = SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Object);
		Conversion conversion;
		if (value.Type is TypeParameterSymbol { AllowsRefLikeType: not false })
		{
			conversion = Conversion.Boxing;
		}
		else
		{
			CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
			conversion = Compilation.Conversions.ClassifyConversionFromExpression(value, namedTypeSymbol, isChecked: false, ref useSiteInfo);
		}
		return ObjectNotEqual(Convert(namedTypeSymbol, value, conversion), Null(namedTypeSymbol));
	}

	public BoundBinaryOperator ObjectNotEqual(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.ObjectNotEqual, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator IntNotEqual(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.IntNotEqual, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator IntLessThan(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.IntLessThan, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator IntGreaterThanOrEqual(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.IntGreaterThanOrEqual, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean), left, right);
	}

	public BoundBinaryOperator IntSubtract(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.IntSubtraction, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32), left, right);
	}

	public BoundBinaryOperator IntMultiply(BoundExpression left, BoundExpression right)
	{
		return Binary(BinaryOperatorKind.IntMultiplication, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32), left, right);
	}

	public BoundLiteral Literal(byte value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Byte))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral Literal(int value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral Literal(StateMachineState value)
	{
		return Literal((int)value);
	}

	public BoundLiteral Literal(uint value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), SpecialType(Microsoft.CodeAnalysis.SpecialType.System_UInt32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral Literal(ConstantValue value, TypeSymbol type)
	{
		return new BoundLiteral(Syntax, value, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundObjectCreationExpression New(NamedTypeSymbol type, params BoundExpression[] args)
	{
		MethodSymbol ctor = type.InstanceConstructors.Single((MethodSymbol c) => c.ParameterCount == args.Length);
		return New(ctor, args);
	}

	public BoundObjectCreationExpression New(MethodSymbol ctor, params BoundExpression[] args)
	{
		return New(ctor, args.ToImmutableArray());
	}

	public BoundObjectCreationExpression New(NamedTypeSymbol type, ImmutableArray<BoundExpression> args)
	{
		MethodSymbol ctor = type.InstanceConstructors.Single((MethodSymbol c) => c.ParameterCount == args.Length);
		return New(ctor, args);
	}

	public BoundObjectCreationExpression New(MethodSymbol ctor, ImmutableArray<BoundExpression> args)
	{
		return new BoundObjectCreationExpression(Syntax, ctor, args)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundObjectCreationExpression New(MethodSymbol constructor, ImmutableArray<BoundExpression> arguments, ImmutableArray<RefKind> argumentRefKinds)
	{
		return new BoundObjectCreationExpression(Syntax, constructor, arguments, default(ImmutableArray<string>), argumentRefKinds, expanded: false, default(ImmutableArray<int>), default(BitVector), null, null, constructor.ContainingType)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundObjectCreationExpression New(WellKnownMember wm, ImmutableArray<BoundExpression> args)
	{
		MethodSymbol constructor = WellKnownMethod(wm);
		return new BoundObjectCreationExpression(Syntax, constructor, args)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression MakeIsNotANumberTest(BoundExpression input)
	{
		TypeSymbol type = input.Type;
		if ((object)type != null)
		{
			switch (type.SpecialType)
			{
			case Microsoft.CodeAnalysis.SpecialType.System_Double:
				return StaticCall(Microsoft.CodeAnalysis.SpecialMember.System_Double__IsNaN, input);
			case Microsoft.CodeAnalysis.SpecialType.System_Single:
				return StaticCall(Microsoft.CodeAnalysis.SpecialMember.System_Single__IsNaN, input);
			}
		}
		throw ExceptionUtilities.UnexpectedValue(input.Type);
	}

	public BoundExpression StaticCall(TypeSymbol receiver, MethodSymbol method, params BoundExpression[] args)
	{
		if ((object)method == null)
		{
			return new BoundBadExpression(Syntax, LookupResultKind.Empty, ImmutableArray<Symbol>.Empty, args.AsImmutable(), receiver);
		}
		return Call(null, method, args);
	}

	public BoundExpression StaticCall(MethodSymbol method, ImmutableArray<BoundExpression> args)
	{
		return Call(null, method, args);
	}

	public BoundExpression StaticCall(WellKnownMember method, params BoundExpression[] args)
	{
		MethodSymbol methodSymbol = WellKnownMethod(method);
		Binder.ReportUseSite(methodSymbol, Diagnostics, Syntax);
		return Call(null, methodSymbol, args);
	}

	public BoundExpression StaticCall(WellKnownMember method, ImmutableArray<TypeSymbol> typeArgs, params BoundExpression[] args)
	{
		MethodSymbol methodSymbol = WellKnownMethod(method);
		Binder.ReportUseSite(methodSymbol, Diagnostics, Syntax);
		return Call(null, methodSymbol.Construct(typeArgs), args);
	}

	public BoundExpression StaticCall(SpecialMember method, params BoundExpression[] args)
	{
		MethodSymbol method2 = SpecialMethod(method);
		return Call(null, method2, args);
	}

	public BoundCall Call(BoundExpression? receiver, MethodSymbol method)
	{
		return Call(receiver, method, ImmutableArray<BoundExpression>.Empty);
	}

	public BoundCall Call(BoundExpression? receiver, MethodSymbol method, BoundExpression arg0, bool useStrictArgumentRefKinds = false)
	{
		return Call(receiver, method, ImmutableArray.Create(arg0), useStrictArgumentRefKinds);
	}

	public BoundCall Call(BoundExpression? receiver, MethodSymbol method, BoundExpression arg0, BoundExpression arg1, bool useStrictArgumentRefKinds = false)
	{
		return Call(receiver, method, ImmutableArray.Create(arg0, arg1), useStrictArgumentRefKinds);
	}

	public BoundCall Call(BoundExpression? receiver, MethodSymbol method, params BoundExpression[] args)
	{
		return Call(receiver, method, ImmutableArray.Create(args));
	}

	public BoundCall Call(BoundExpression? receiver, WellKnownMember method, BoundExpression arg0)
	{
		return Call(receiver, WellKnownMethod(method), ImmutableArray.Create(arg0));
	}

	public BoundCall Call(BoundExpression? receiver, MethodSymbol method, ImmutableArray<BoundExpression> args, bool useStrictArgumentRefKinds = false)
	{
		return new BoundCall(Syntax, receiver, ThreeState.Unknown, method, args, default(ImmutableArray<string>), ArgumentRefKindsFromParameterRefKinds(method, useStrictArgumentRefKinds), isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default(ImmutableArray<int>), default(BitVector), LookupResultKind.Viable, method.ReturnType, method.OriginalDefinition is ErrorMethodSymbol)
		{
			WasCompilerGenerated = true
		};
	}

	public static ImmutableArray<RefKind> ArgumentRefKindsFromParameterRefKinds(MethodSymbol method, bool useStrictArgumentRefKinds)
	{
		ImmutableArray<RefKind> parameterRefKinds = method.ParameterRefKinds;
		if (!parameterRefKinds.IsDefaultOrEmpty && (parameterRefKinds.Contains(RefKind.RefReadOnlyParameter) || (useStrictArgumentRefKinds && parameterRefKinds.Contains(RefKind.In))))
		{
			ArrayBuilder<RefKind> instance = ArrayBuilder<RefKind>.GetInstance(parameterRefKinds.Length);
			foreach (RefKind item in parameterRefKinds)
			{
				instance.Add(ArgumentRefKindFromParameterRefKind(item, useStrictArgumentRefKinds));
			}
			return instance.ToImmutableAndFree();
		}
		return parameterRefKinds;
	}

	public static RefKind ArgumentRefKindFromParameterRefKind(RefKind refKind, bool useStrictArgumentRefKinds)
	{
		RefKind refKind2 = refKind;
		int num;
		if (refKind2 != RefKind.In)
		{
			if (refKind2 != RefKind.RefReadOnlyParameter)
			{
				goto IL_0024;
			}
			num = 1;
		}
		else
		{
			num = 0;
		}
		if (!useStrictArgumentRefKinds)
		{
			if (num == 0)
			{
				goto IL_0024;
			}
			if (num == 1)
			{
				return RefKind.In;
			}
		}
		return (RefKind)5;
		IL_0024:
		return refKind;
	}

	public BoundCall Call(BoundExpression? receiver, MethodSymbol method, ImmutableArray<RefKind> refKinds, ImmutableArray<BoundExpression> args)
	{
		return new BoundCall(Syntax, receiver, ThreeState.Unknown, method, args, default(ImmutableArray<string>), refKinds, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, ImmutableArray<int>.Empty, default(BitVector), LookupResultKind.Viable, method.ReturnType)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression Conditional(BoundExpression condition, BoundExpression consequence, BoundExpression alternative, TypeSymbol type, bool isRef = false)
	{
		return new BoundConditionalOperator(Syntax, isRef, condition, consequence, alternative, null, type, wasTargetTyped: false, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundComplexConditionalReceiver ComplexConditionalReceiver(BoundExpression valueTypeReceiver, BoundExpression referenceTypeReceiver)
	{
		return new BoundComplexConditionalReceiver(Syntax, valueTypeReceiver, referenceTypeReceiver, valueTypeReceiver.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression Coalesce(BoundExpression left, BoundExpression right)
	{
		return new BoundNullCoalescingOperator(Syntax, left, right, null, null, BoundNullCoalescingOperatorResultKind.LeftType, @checked: false, left.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundStatement If(BoundExpression condition, BoundStatement thenClause, BoundStatement? elseClauseOpt = null)
	{
		return If(condition, ImmutableArray<LocalSymbol>.Empty, thenClause, elseClauseOpt);
	}

	public BoundStatement ConditionalGoto(BoundExpression condition, LabelSymbol label, bool jumpIfTrue)
	{
		return new BoundConditionalGoto(Syntax, condition, jumpIfTrue, label)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundStatement If(BoundExpression condition, ImmutableArray<LocalSymbol> locals, BoundStatement thenClause, BoundStatement? elseClauseOpt = null)
	{
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("afterif");
		if (elseClauseOpt != null)
		{
			GeneratedLabelSymbol label2 = new GeneratedLabelSymbol("alternative");
			instance.Add(ConditionalGoto(condition, label2, jumpIfTrue: false));
			instance.Add(thenClause);
			instance.Add(Goto(label));
			if (!locals.IsDefaultOrEmpty)
			{
				BoundBlock item = Block(locals, instance.ToImmutable());
				instance.Clear();
				instance.Add(item);
			}
			instance.Add(Label(label2));
			instance.Add(elseClauseOpt);
		}
		else
		{
			instance.Add(ConditionalGoto(condition, label, jumpIfTrue: false));
			instance.Add(thenClause);
			if (!locals.IsDefaultOrEmpty)
			{
				BoundBlock item2 = Block(locals, instance.ToImmutable());
				instance.Clear();
				instance.Add(item2);
			}
		}
		instance.Add(Label(label));
		return Block(instance.ToImmutableAndFree());
	}

	public BoundThrowStatement Throw(BoundExpression e)
	{
		return new BoundThrowStatement(Syntax, e)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLocal Local(LocalSymbol local)
	{
		return new BoundLocal(Syntax, local, null, local.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression MakeSequence(LocalSymbol temp, params BoundExpression[] parts)
	{
		return MakeSequence(ImmutableArray.Create(temp), parts);
	}

	public BoundExpression MakeSequence(params BoundExpression[] parts)
	{
		return MakeSequence(ImmutableArray<LocalSymbol>.Empty, parts);
	}

	public BoundExpression MakeSequence(ImmutableArray<LocalSymbol> locals, params BoundExpression[] parts)
	{
		ArrayBuilder<BoundExpression> instance = ArrayBuilder<BoundExpression>.GetInstance();
		for (int i = 0; i < parts.Length - 1; i++)
		{
			if (LocalRewriter.ReadIsSideeffecting(parts[i]))
			{
				instance.Add(parts[i]);
			}
		}
		BoundExpression result = parts[^1];
		if (locals.IsDefaultOrEmpty && instance.Count == 0)
		{
			instance.Free();
			return result;
		}
		return Sequence(locals, instance.ToImmutableAndFree(), result);
	}

	public BoundSequence Sequence(BoundExpression[] sideEffects, BoundExpression result, TypeSymbol? type = null)
	{
		TypeSymbol type2 = type ?? result.Type;
		return new BoundSequence(Syntax, ImmutableArray<LocalSymbol>.Empty, sideEffects.AsImmutableOrNull(), result, type2)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression Sequence(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundExpression> sideEffects, BoundExpression result)
	{
		if (!locals.IsDefaultOrEmpty || !sideEffects.IsDefaultOrEmpty)
		{
			return new BoundSequence(Syntax, locals, sideEffects, result, result.Type)
			{
				WasCompilerGenerated = true
			};
		}
		return result;
	}

	public BoundSpillSequence SpillSequence(ImmutableArray<LocalSymbol> locals, ImmutableArray<BoundStatement> sideEffects, BoundExpression result)
	{
		return new BoundSpillSequence(Syntax, locals, sideEffects, result, result.Type)
		{
			WasCompilerGenerated = true
		};
	}

	public SyntheticSwitchSection SwitchSection(int value, params BoundStatement[] statements)
	{
		return SwitchSection(ImmutableArray.Create(value), statements);
	}

	public SyntheticSwitchSection SwitchSection(ImmutableArray<int> values, params BoundStatement[] statements)
	{
		return new SyntheticSwitchSection(values, ImmutableArray.Create(statements));
	}

	public BoundStatement Switch(BoundExpression ex, ImmutableArray<SyntheticSwitchSection> sections)
	{
		if (sections.Length == 0)
		{
			return ExpressionStatement(ex);
		}
		GeneratedLabelSymbol generatedLabelSymbol = new GeneratedLabelSymbol("break");
		ArrayBuilder<(ConstantValue, LabelSymbol)> instance = ArrayBuilder<(ConstantValue, LabelSymbol)>.GetInstance();
		ArrayBuilder<BoundStatement> instance2 = ArrayBuilder<BoundStatement>.GetInstance();
		instance2.Add(null);
		foreach (SyntheticSwitchSection item in sections)
		{
			LabelSymbol labelSymbol = new GeneratedLabelSymbol("case " + item.Values[0]);
			instance2.Add(Label(labelSymbol));
			instance2.AddRange(item.Statements);
			foreach (int value in item.Values)
			{
				instance.Add((ConstantValue.Create(value), labelSymbol));
			}
		}
		instance2.Add(Label(generatedLabelSymbol));
		instance2[0] = new BoundSwitchDispatch(Syntax, ex, instance.ToImmutableAndFree(), generatedLabelSymbol, null)
		{
			WasCompilerGenerated = true
		};
		return Block(instance2.ToImmutableAndFree());
	}

	[Conditional("DEBUG")]
	private static void CheckSwitchSections(ImmutableArray<SyntheticSwitchSection> sections)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (SyntheticSwitchSection item in sections)
		{
			foreach (int value in item.Values)
			{
				hashSet.Add(value);
			}
		}
	}

	public BoundGotoStatement Goto(LabelSymbol label)
	{
		return new BoundGotoStatement(Syntax, label)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLabelStatement Label(LabelSymbol label)
	{
		return new BoundLabelStatement(Syntax, label)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral Literal(bool value)
	{
		return new BoundLiteral(Syntax, ConstantValue.Create(value), SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral Literal(string? value)
	{
		ConstantValue stringConst = ConstantValue.Create(value);
		return StringLiteral(stringConst);
	}

	public BoundLiteral StringLiteral(ConstantValue stringConst)
	{
		return new BoundLiteral(Syntax, stringConst, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_String))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral StringLiteral(string stringValue)
	{
		return StringLiteral(ConstantValue.Create(stringValue));
	}

	public BoundLiteral CharLiteral(ConstantValue charConst)
	{
		return new BoundLiteral(Syntax, charConst, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Char))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundLiteral CharLiteral(char charValue)
	{
		return CharLiteral(ConstantValue.Create(charValue));
	}

	public BoundArrayLength ArrayLength(BoundExpression array)
	{
		return new BoundArrayLength(Syntax, array, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32));
	}

	public BoundArrayAccess ArrayAccessFirstElement(BoundExpression array)
	{
		ImmutableArray<BoundExpression> indices = ArrayBuilder<BoundExpression>.GetInstance(((ArrayTypeSymbol)array.Type).Rank, Literal(0)).ToImmutableAndFree();
		return ArrayAccess(array, indices);
	}

	public BoundArrayAccess ArrayAccess(BoundExpression array, params BoundExpression[] indices)
	{
		return ArrayAccess(array, indices.AsImmutableOrNull());
	}

	public BoundArrayAccess ArrayAccess(BoundExpression array, ImmutableArray<BoundExpression> indices)
	{
		return new BoundArrayAccess(Syntax, array, indices, ((ArrayTypeSymbol)array.Type).ElementType);
	}

	public BoundStatement BaseInitialization()
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = CurrentFunction.ThisParameter.Type.BaseTypeNoUseSiteDiagnostics;
		MethodSymbol method = baseTypeNoUseSiteDiagnostics.InstanceConstructors.Single((MethodSymbol c) => c.ParameterCount == 0);
		return new BoundExpressionStatement(Syntax, Call(Base(baseTypeNoUseSiteDiagnostics), method))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundStatement SequencePoint(SyntaxNode syntax, BoundStatement statement)
	{
		return new BoundSequencePoint(syntax, statement);
	}

	public BoundStatement SequencePointWithSpan(CSharpSyntaxNode syntax, TextSpan span, BoundStatement statement)
	{
		return new BoundSequencePointWithSpan(syntax, statement, span);
	}

	public BoundStatement HiddenSequencePoint(BoundStatement? statementOpt = null)
	{
		return BoundSequencePoint.CreateHidden(statementOpt);
	}

	public BoundStatement ThrowNull()
	{
		return Throw(Null(Binder.GetWellKnownType(Compilation, Microsoft.CodeAnalysis.WellKnownType.System_Exception, Diagnostics, Syntax.Location)));
	}

	public BoundExpression ThrowExpression(BoundExpression thrown, TypeSymbol type)
	{
		return new BoundThrowExpression(thrown.Syntax, thrown, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression Null(TypeSymbol type)
	{
		return Null(type, Syntax);
	}

	public BoundExpression NullRef(TypeWithAnnotations type)
	{
		return new BoundPointerIndirectionOperator(Syntax, Default(new PointerTypeSymbol(type)), refersToLocation: false, type.Type);
	}

	public static BoundExpression Null(TypeSymbol type, SyntaxNode syntax)
	{
		BoundExpression boundExpression = new BoundLiteral(syntax, ConstantValue.Null, type)
		{
			WasCompilerGenerated = true
		};
		if (!type.IsPointerOrFunctionPointer())
		{
			return boundExpression;
		}
		return BoundConversion.SynthesizedNonUserDefined(syntax, boundExpression, Conversion.NullToPointer, type);
	}

	public BoundTypeExpression Type(TypeSymbol type)
	{
		return new BoundTypeExpression(Syntax, null, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression Typeof(WellKnownType type, TypeSymbol systemType)
	{
		return Typeof(WellKnownType(type), systemType);
	}

	public BoundExpression Typeof(TypeSymbol type, TypeSymbol systemType)
	{
		return new BoundTypeOfOperator(getTypeFromHandle: (!(systemType.ExtendedSpecialType == InternalSpecialType.System_Type)) ? WellKnownMethod(Microsoft.CodeAnalysis.WellKnownMember.System_Type__GetTypeFromHandle) : SpecialMethod(Microsoft.CodeAnalysis.SpecialMember.System_Type__GetTypeFromHandle), syntax: Syntax, sourceType: Type(type), type: systemType)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression Typeof(TypeWithAnnotations type, TypeSymbol systemType)
	{
		return Typeof(type.Type, systemType);
	}

	public ImmutableArray<BoundExpression> TypeOfs(ImmutableArray<TypeWithAnnotations> typeArguments, TypeSymbol systemType)
	{
		return typeArguments.SelectAsArray(Typeof, systemType);
	}

	public BoundExpression TypeofDynamicOperationContextType()
	{
		return Typeof(CompilationState.DynamicOperationContextType, WellKnownType(Microsoft.CodeAnalysis.WellKnownType.System_Type));
	}

	public BoundExpression Sizeof(TypeSymbol type)
	{
		return new BoundSizeOfOperator(Syntax, Type(type), Binder.GetConstantSizeOf(type), SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	internal BoundExpression ConstructorInfo(MethodSymbol ctor)
	{
		NamedTypeSymbol namedTypeSymbol = WellKnownType(Microsoft.CodeAnalysis.WellKnownType.System_Reflection_ConstructorInfo);
		return new BoundMethodInfo(Syntax, ctor, GetMethodFromHandleMethod(ctor.ContainingType, namedTypeSymbol), namedTypeSymbol)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression MethodDefIndex(MethodSymbol method)
	{
		return new BoundMethodDefIndex(Syntax, method, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression LocalId(LocalSymbol symbol)
	{
		return new BoundLocalId(Syntax, symbol, null, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression ParameterId(ParameterSymbol symbol)
	{
		return new BoundParameterId(Syntax, symbol, null, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression StateMachineInstanceId()
	{
		return new BoundStateMachineInstanceId(Syntax, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_UInt64))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression ModuleVersionId()
	{
		return new BoundModuleVersionId(Syntax, WellKnownType(Microsoft.CodeAnalysis.WellKnownType.System_Guid))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression ModuleVersionIdString()
	{
		return new BoundModuleVersionIdString(Syntax, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_String))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression InstrumentationPayloadRoot(int analysisKind, TypeSymbol payloadType)
	{
		return new BoundInstrumentationPayloadRoot(Syntax, analysisKind, payloadType)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression ThrowIfModuleCancellationRequested()
	{
		return new BoundThrowIfModuleCancellationRequested(Syntax, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Void))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression ModuleCancellationToken()
	{
		return new ModuleCancellationTokenExpression(Syntax, WellKnownType(Microsoft.CodeAnalysis.WellKnownType.System_Threading_CancellationToken))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression MaximumMethodDefIndex()
	{
		return new BoundMaximumMethodDefIndex(Syntax, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression SourceDocumentIndex(DebugSourceDocument document)
	{
		return new BoundSourceDocumentIndex(Syntax, document, SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Int32))
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression MethodInfo(MethodSymbol method, TypeSymbol systemReflectionMethodInfo)
	{
		if (!method.ContainingType.IsValueType || !CodeGenerator.MayUseCallForStructMethod(method))
		{
			method = method.GetConstructedLeastOverriddenMethod(CompilationState.Type, requireSameReturnType: true);
		}
		return new BoundMethodInfo(Syntax, method, GetMethodFromHandleMethod(method.ContainingType, systemReflectionMethodInfo), systemReflectionMethodInfo)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression FieldInfo(FieldSymbol field)
	{
		return new BoundFieldInfo(Syntax, field, GetFieldFromHandleMethod(field.ContainingType), WellKnownType(Microsoft.CodeAnalysis.WellKnownType.System_Reflection_FieldInfo))
		{
			WasCompilerGenerated = true
		};
	}

	private MethodSymbol GetMethodFromHandleMethod(NamedTypeSymbol methodContainer, TypeSymbol systemReflectionMethodOrConstructorInfo)
	{
		bool flag = methodContainer.AllTypeArgumentCount() == 0 && !methodContainer.IsAnonymousType;
		if (systemReflectionMethodOrConstructorInfo.ExtendedSpecialType == InternalSpecialType.System_Reflection_MethodInfo)
		{
			return SpecialMethod(flag ? Microsoft.CodeAnalysis.SpecialMember.System_Reflection_MethodBase__GetMethodFromHandle : Microsoft.CodeAnalysis.SpecialMember.System_Reflection_MethodBase__GetMethodFromHandle2);
		}
		return WellKnownMethod(flag ? Microsoft.CodeAnalysis.WellKnownMember.System_Reflection_MethodBase__GetMethodFromHandle : Microsoft.CodeAnalysis.WellKnownMember.System_Reflection_MethodBase__GetMethodFromHandle2);
	}

	private MethodSymbol GetFieldFromHandleMethod(NamedTypeSymbol fieldContainer)
	{
		return WellKnownMethod((fieldContainer.AllTypeArgumentCount() == 0) ? Microsoft.CodeAnalysis.WellKnownMember.System_Reflection_FieldInfo__GetFieldFromHandle : Microsoft.CodeAnalysis.WellKnownMember.System_Reflection_FieldInfo__GetFieldFromHandle2);
	}

	public Conversion ClassifyEmitConversion(BoundExpression arg, TypeSymbol destination)
	{
		CompoundUseSiteInfo<AssemblySymbol> useSiteInfo = CompoundUseSiteInfo<AssemblySymbol>.Discarded;
		return Compilation.Conversions.ClassifyConversionFromExpression(arg, destination, isChecked: false, ref useSiteInfo);
	}

	public BoundExpression Convert(TypeSymbol type, BoundExpression arg, Conversion conversion, bool isChecked = false)
	{
		if (TypeSymbol.Equals(type, arg.Type, TypeCompareKind.ConsiderEverything))
		{
			return arg;
		}
		if (conversion.Kind == ConversionKind.ImplicitReference && arg.IsLiteralNull())
		{
			return Null(type);
		}
		return new BoundConversion(Syntax, arg, conversion, isChecked, explicitCastInCode: true, null, null, type)
		{
			WasCompilerGenerated = true
		};
	}

	public BoundExpression ArrayOrEmpty(TypeSymbol elementType, BoundExpression[] elements)
	{
		return ArrayOrEmpty(elementType, elements.AsImmutable());
	}

	public BoundExpression ArrayOrEmpty(TypeSymbol elementType, ImmutableArray<BoundExpression> elements)
	{
		if (elements.Length == 0)
		{
			MethodSymbol methodSymbol = SpecialMethod(Microsoft.CodeAnalysis.SpecialMember.System_Array__Empty, isOptional: true);
			if ((object)methodSymbol != null)
			{
				methodSymbol = methodSymbol.Construct(ImmutableArray.Create(elementType));
				return Call(null, methodSymbol);
			}
		}
		return Array(elementType, elements);
	}

	public BoundExpression Array(TypeSymbol elementType, ImmutableArray<BoundExpression> elements)
	{
		return new BoundArrayCreation(Syntax, ImmutableArray.Create((BoundExpression)Literal(elements.Length)), new BoundArrayInitialization(Syntax, isInferred: false, elements)
		{
			WasCompilerGenerated = true
		}, Compilation.CreateArrayTypeSymbol(elementType));
	}

	public BoundExpression Array(TypeSymbol elementType, BoundExpression length)
	{
		return new BoundArrayCreation(Syntax, ImmutableArray.Create(length), null, Compilation.CreateArrayTypeSymbol(elementType))
		{
			WasCompilerGenerated = true
		};
	}

	internal BoundExpression Default(TypeSymbol type)
	{
		return Default(type, Syntax);
	}

	internal static BoundExpression Default(TypeSymbol type, SyntaxNode syntax)
	{
		return new BoundDefaultExpression(syntax, type)
		{
			WasCompilerGenerated = true
		};
	}

	internal BoundStatement Try(BoundBlock tryBlock, ImmutableArray<BoundCatchBlock> catchBlocks, BoundBlock? finallyBlock = null, LabelSymbol? finallyLabel = null)
	{
		return new BoundTryStatement(Syntax, tryBlock, catchBlocks, finallyBlock, finallyLabel)
		{
			WasCompilerGenerated = true
		};
	}

	internal ImmutableArray<BoundCatchBlock> CatchBlocks(params BoundCatchBlock[] catchBlocks)
	{
		return catchBlocks.AsImmutableOrNull();
	}

	internal BoundCatchBlock Catch(LocalSymbol local, BoundBlock block)
	{
		BoundLocal boundLocal = Local(local);
		return new BoundCatchBlock(Syntax, ImmutableArray.Create(local), boundLocal, boundLocal.Type, null, null, block, isSynthesizedAsyncCatchAll: false);
	}

	internal BoundCatchBlock Catch(BoundExpression source, BoundBlock block)
	{
		return new BoundCatchBlock(Syntax, ImmutableArray<LocalSymbol>.Empty, source, source.Type, null, null, block, isSynthesizedAsyncCatchAll: false);
	}

	internal BoundTryStatement Fault(BoundBlock tryBlock, BoundBlock faultBlock)
	{
		return new BoundTryStatement(Syntax, tryBlock, ImmutableArray<BoundCatchBlock>.Empty, faultBlock, null, preferFaultHandler: true);
	}

	internal BoundExpression NullOrDefault(TypeSymbol typeSymbol)
	{
		return NullOrDefault(typeSymbol, Syntax);
	}

	internal static BoundExpression NullOrDefault(TypeSymbol typeSymbol, SyntaxNode syntax)
	{
		if (!typeSymbol.IsReferenceType)
		{
			return Default(typeSymbol, syntax);
		}
		return Null(typeSymbol, syntax);
	}

	internal BoundExpression Not(BoundExpression expression)
	{
		return new BoundUnaryOperator(expression.Syntax, UnaryOperatorKind.BoolLogicalNegation, expression, null, null, null, LookupResultKind.Viable, expression.Type);
	}

	public BoundLocal StoreToTemp(BoundExpression argument, out BoundAssignmentOperator store, RefKind refKind = RefKind.None, SynthesizedLocalKind kind = SynthesizedLocalKind.LoweringTemp, bool isKnownToReferToTempIfReferenceType = false, SyntaxNode? syntaxOpt = null)
	{
		MethodSymbol currentFunction = CurrentFunction;
		switch (refKind)
		{
		case RefKind.Out:
			refKind = RefKind.Ref;
			break;
		case RefKind.In:
			if (!CodeGenerator.HasHome(argument, CodeGenerator.AddressKind.ReadOnly, currentFunction, Compilation.IsPeVerifyCompatEnabled, null))
			{
				refKind = RefKind.None;
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(refKind);
		case RefKind.None:
		case RefKind.Ref:
		case (RefKind)5:
			break;
		}
		SyntaxNode syntax = argument.Syntax;
		TypeSymbol type = argument.Type;
		BoundLocal boundLocal = new BoundLocal(syntax, new SynthesizedLocal(currentFunction, TypeWithAnnotations.Create(type), kind, syntaxOpt ?? (kind.IsLongLived() ? syntax : null), isPinned: false, isKnownToReferToTempIfReferenceType, refKind), null, type);
		store = new BoundAssignmentOperator(syntax, boundLocal, argument, type, refKind != RefKind.None);
		return boundLocal;
	}

	internal BoundStatement NoOp(NoOpStatementFlavor noOpStatementFlavor)
	{
		return new BoundNoOpStatement(Syntax, noOpStatementFlavor);
	}

	internal BoundLocal MakeTempForDiscard(BoundDiscardExpression node, ArrayBuilder<LocalSymbol> temps)
	{
		BoundLocal result = MakeTempForDiscard(node, out LocalSymbol temp);
		temps.Add(temp);
		return result;
	}

	internal BoundLocal MakeTempForDiscard(BoundDiscardExpression node, out LocalSymbol temp)
	{
		temp = new SynthesizedLocal(CurrentFunction, TypeWithAnnotations.Create(node.Type), SynthesizedLocalKind.LoweringTemp);
		return new BoundLocal(node.Syntax, temp, null, node.Type)
		{
			WasCompilerGenerated = true
		};
	}

	internal ImmutableArray<BoundExpression> MakeTempsForDiscardArguments(ImmutableArray<BoundExpression> arguments, ArrayBuilder<LocalSymbol> builder)
	{
		if (arguments.Any((BoundExpression a) => a.Kind == BoundKind.DiscardExpression))
		{
			arguments = arguments.SelectAsArray((BoundExpression arg, (SyntheticBoundNodeFactory factory, ArrayBuilder<LocalSymbol> builder) t) => (arg.Kind != BoundKind.DiscardExpression) ? arg : t.factory.MakeTempForDiscard((BoundDiscardExpression)arg, t.builder), (this, builder));
		}
		return arguments;
	}

	internal BoundExpression MakeNullCheck(SyntaxNode syntax, BoundExpression rewrittenExpr, BinaryOperatorKind operatorKind)
	{
		TypeSymbol type = rewrittenExpr.Type;
		TypeSymbol specialType = Compilation.GetSpecialType(Microsoft.CodeAnalysis.SpecialType.System_Boolean);
		if (rewrittenExpr.ConstantValueOpt != null)
		{
			switch (operatorKind)
			{
			case BinaryOperatorKind.Equal:
				return Literal(ConstantValue.Create(rewrittenExpr.ConstantValueOpt.IsNull, ConstantValueTypeDiscriminator.Boolean), specialType);
			case BinaryOperatorKind.NotEqual:
				return Literal(ConstantValue.Create(rewrittenExpr.ConstantValueOpt.IsNull, ConstantValueTypeDiscriminator.Boolean), specialType);
			}
		}
		TypeSymbol type2 = SpecialType(Microsoft.CodeAnalysis.SpecialType.System_Object);
		if ((object)type != null)
		{
			if (type.Kind == SymbolKind.TypeParameter)
			{
				rewrittenExpr = Convert(type2, rewrittenExpr, Conversion.Boxing);
			}
			else if (type.IsNullableType())
			{
				operatorKind |= BinaryOperatorKind.NullableNull;
			}
		}
		if (operatorKind == BinaryOperatorKind.NullableNullEqual || operatorKind == BinaryOperatorKind.NullableNullNotEqual)
		{
			return RewriteNullableNullEquality(syntax, operatorKind, rewrittenExpr, Literal(ConstantValue.Null, type2), specialType);
		}
		return Binary(operatorKind, specialType, rewrittenExpr, Null(type2));
	}

	internal BoundExpression MakeNullableHasValue(SyntaxNode syntax, BoundExpression expression)
	{
		return BoundCall.Synthesized(syntax, expression, ThreeState.Unknown, LocalRewriter.UnsafeGetNullableMethod(syntax, expression.Type, Microsoft.CodeAnalysis.SpecialMember.System_Nullable_T_get_HasValue, Compilation, Diagnostics));
	}

	internal BoundExpression RewriteNullableNullEquality(SyntaxNode syntax, BinaryOperatorKind kind, BoundExpression loweredLeft, BoundExpression loweredRight, TypeSymbol returnType)
	{
		BoundExpression boundExpression = (loweredRight.IsLiteralNull() ? loweredLeft : loweredRight);
		if (LocalRewriter.NullableNeverHasValue(boundExpression))
		{
			return Literal(kind == BinaryOperatorKind.NullableNullEqual);
		}
		BoundExpression boundExpression2 = LocalRewriter.NullableAlwaysHasValue(boundExpression);
		if (boundExpression2 != null)
		{
			return new BoundSequence(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundExpression2), Literal(kind == BinaryOperatorKind.NullableNullNotEqual), returnType);
		}
		if (boundExpression is BoundLoweredConditionalAccess boundLoweredConditionalAccess && (boundLoweredConditionalAccess.WhenNullOpt == null || boundLoweredConditionalAccess.WhenNullOpt.IsDefaultValue()))
		{
			BoundExpression boundExpression3 = RewriteNullableNullEquality(syntax, kind, boundLoweredConditionalAccess.WhenNotNull, loweredLeft.IsLiteralNull() ? loweredLeft : loweredRight, returnType);
			BoundLiteral whenNullOpt = ((kind == BinaryOperatorKind.NullableNullEqual) ? Literal(value: true) : null);
			return boundLoweredConditionalAccess.Update(boundLoweredConditionalAccess.Receiver, boundLoweredConditionalAccess.HasValueMethodOpt, boundExpression3, whenNullOpt, boundLoweredConditionalAccess.Id, boundLoweredConditionalAccess.ForceCopyOfNullableValueType, boundExpression3.Type);
		}
		BoundExpression boundExpression4 = MakeNullableHasValue(syntax, boundExpression);
		if (kind != BinaryOperatorKind.NullableNullNotEqual)
		{
			return new BoundUnaryOperator(syntax, UnaryOperatorKind.BoolLogicalNegation, boundExpression4, null, null, null, LookupResultKind.Viable, returnType);
		}
		return boundExpression4;
	}
}
