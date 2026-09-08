using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.Emit;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class DataSectionStringType : NestedTypeDefinition
{
	private sealed class DataSectionStringField : SynthesizedStaticFieldBase
	{
		public override ImmutableArray<byte> MappedData => default(ImmutableArray<byte>);

		public override bool IsReadOnly => true;

		public DataSectionStringField(string name, INamedTypeDefinition containingType)
			: base(name, containingType)
		{
		}

		public override ITypeReference GetType(EmitContext context)
		{
			return context.Module.GetPlatformType(PlatformType.SystemString, context);
		}

		public override string ToString()
		{
			return $"string {((object)base.ContainingTypeDefinition.GetInternalSymbol()) ?? ((object)base.ContainingTypeDefinition)}.{base.Name}";
		}
	}

	private readonly string _name;

	private readonly PrivateImplementationDetails _containingType;

	private readonly ImmutableArray<IFieldDefinition> _fields;

	private readonly ImmutableArray<IMethodDefinition> _methods;

	public IFieldDefinition Field => _fields[0];

	public override string Name => _name;

	public override ITypeDefinition ContainingTypeDefinition => _containingType;

	public override TypeMemberVisibility Visibility => TypeMemberVisibility.Assembly;

	public override bool IsBeforeFieldInit => true;

	public DataSectionStringType(string name, PrivateImplementationDetails containingType, MappedField dataField, IMethodDefinition bytesToStringHelper, DiagnosticBag diagnostics)
	{
		_name = name;
		_containingType = containingType;
		DataSectionStringField dataSectionStringField = new DataSectionStringField("s", this);
		IMethodDefinition methodDefinition = synthesizeStaticConstructor(containingType.ModuleBuilder, this, dataField, dataSectionStringField, bytesToStringHelper, diagnostics);
		_fields = ImmutableCollectionsMarshal.AsImmutableArray(new IFieldDefinition[1] { dataSectionStringField });
		_methods = ImmutableCollectionsMarshal.AsImmutableArray(new IMethodDefinition[1] { methodDefinition });
		static IMethodDefinition synthesizeStaticConstructor(CommonPEModuleBuilder module, ITypeDefinition containingTypeDefinition, MappedField mappedField, DataSectionStringField stringField, IMethodDefinition value, DiagnosticBag diagnostics2)
		{
			ILBuilder iLBuilder = new ILBuilder(module, new LocalSlotManager(null), diagnostics2, OptimizationLevel.Release, areLocalsZeroed: false);
			iLBuilder.EmitOpCode(ILOpCode.Ldsflda);
			iLBuilder.EmitToken(mappedField, null);
			iLBuilder.EmitIntConstant(mappedField.MappedData.Length);
			iLBuilder.EmitOpCode(ILOpCode.Call, -1);
			iLBuilder.EmitToken((ISignature)value, (SyntaxNode?)null);
			iLBuilder.EmitOpCode(ILOpCode.Stsfld);
			iLBuilder.EmitToken(stringField, null);
			iLBuilder.EmitRet(isVoid: true);
			iLBuilder.Realize();
			return new StaticConstructor(containingTypeDefinition, iLBuilder.MaxStack, iLBuilder.RealizedIL);
		}
	}

	public override ITypeReference GetBaseClass(EmitContext context)
	{
		return _containingType.SystemObject;
	}

	public override IEnumerable<IFieldDefinition> GetFields(EmitContext context)
	{
		return _fields;
	}

	public override IEnumerable<IMethodDefinition> GetMethods(EmitContext context)
	{
		return _methods;
	}
}
