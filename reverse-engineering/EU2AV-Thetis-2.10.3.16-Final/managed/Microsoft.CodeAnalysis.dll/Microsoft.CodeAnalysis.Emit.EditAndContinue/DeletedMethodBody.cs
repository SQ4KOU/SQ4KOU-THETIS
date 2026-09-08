using System.Collections.Immutable;
using System.Reflection.Metadata;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Debugging;
using Microsoft.CodeAnalysis.Symbols;

namespace Microsoft.CodeAnalysis.Emit.EditAndContinue;

internal sealed class DeletedMethodBody(IDeletedMethodDefinition methodDef, ImmutableArray<byte> il) : IMethodBody
{
	private readonly IDeletedMethodDefinition _methodDef = methodDef;

	public ImmutableArray<byte> IL { get; } = il;

	public ImmutableArray<ExceptionHandlerRegion> ExceptionRegions => ImmutableArray<ExceptionHandlerRegion>.Empty;

	public bool AreLocalsZeroed => false;

	public bool HasStackalloc => false;

	public ImmutableArray<ILocalDefinition> LocalVariables => ImmutableArray<ILocalDefinition>.Empty;

	public IMethodDefinition MethodDefinition => _methodDef;

	public StateMachineMoveNextBodyDebugInfo MoveNextBodyInfo => null;

	public ushort MaxStack => 8;

	public ImmutableArray<Microsoft.Cci.SequencePoint> SequencePoints => ImmutableArray<Microsoft.Cci.SequencePoint>.Empty;

	public bool HasDynamicLocalVariables => false;

	public ImmutableArray<Microsoft.Cci.LocalScope> LocalScopes => ImmutableArray<Microsoft.Cci.LocalScope>.Empty;

	public Microsoft.Cci.IImportScope ImportScope => null;

	public DebugId MethodId => default(DebugId);

	public ImmutableArray<StateMachineHoistedLocalScope> StateMachineHoistedLocalScopes => default(ImmutableArray<StateMachineHoistedLocalScope>);

	public string StateMachineTypeName => null;

	public ImmutableArray<EncHoistedLocalInfo> StateMachineHoistedLocalSlots => default(ImmutableArray<EncHoistedLocalInfo>);

	public ImmutableArray<ITypeReference> StateMachineAwaiterSlots => default(ImmutableArray<ITypeReference>);

	public ImmutableArray<EncClosureInfo> ClosureDebugInfo => ImmutableArray<EncClosureInfo>.Empty;

	public ImmutableArray<EncLambdaInfo> LambdaDebugInfo => ImmutableArray<EncLambdaInfo>.Empty;

	public ImmutableArray<LambdaRuntimeRudeEditInfo> OrderedLambdaRuntimeRudeEdits => ImmutableArray<LambdaRuntimeRudeEditInfo>.Empty;

	public ImmutableArray<SourceSpan> CodeCoverageSpans => ImmutableArray<SourceSpan>.Empty;

	public StateMachineStatesDebugInfo StateMachineStatesDebugInfo => default(StateMachineStatesDebugInfo);

	public bool IsPrimaryConstructor => false;

	public static ImmutableArray<byte> GetIL(EmitContext context, RuntimeRudeEdit? rudeEdit, bool isLambdaOrLocalFunction)
	{
		IMethodSymbolInternal orCreateHotReloadExceptionConstructorDefinition = context.Module.GetOrCreateHotReloadExceptionConstructorDefinition();
		ILBuilder iLBuilder = new ILBuilder(context.Module, null, context.Diagnostics, OptimizationLevel.Debug, areLocalsZeroed: false);
		string value;
		int value2;
		if (rudeEdit.HasValue)
		{
			value = string.Format(CodeAnalysisResources.EncLambdaRudeEdit, rudeEdit.Value.Message);
			value2 = rudeEdit.Value.ErrorCode;
		}
		else
		{
			int code = (isLambdaOrLocalFunction ? 1 : 2);
			value = ((HotReloadExceptionCode)code).GetExceptionMessage();
			value2 = ((HotReloadExceptionCode)code).GetExceptionCodeValue();
		}
		SyntaxNode syntaxNode = context.SyntaxNode;
		iLBuilder.EmitStringConstant(value, syntaxNode);
		iLBuilder.EmitIntConstant(value2);
		iLBuilder.EmitOpCode(ILOpCode.Newobj, -1);
		iLBuilder.EmitToken(orCreateHotReloadExceptionConstructorDefinition.GetCciAdapter(), syntaxNode);
		iLBuilder.EmitThrow(isRethrow: false);
		iLBuilder.Realize();
		return iLBuilder.RealizedIL;
	}
}
