using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedBackingFieldSymbolBase : FieldSymbolWithAttributesAndModifiers
{
	private readonly string _name;

	internal abstract bool HasInitializer { get; }

	protected override DeclarationModifiers Modifiers { get; }

	public override string Name => _name;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override bool HasRuntimeSpecialName => false;

	public override bool IsImplicitlyDeclared => true;

	internal override bool IsRequired => false;

	public SynthesizedBackingFieldSymbolBase(string name, bool isReadOnly, bool isStatic)
	{
		_name = name;
		Modifiers = (DeclarationModifiers)(0x100 | (isReadOnly ? 1024 : 0) | (isStatic ? 4 : 0));
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<CSharpAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		if (!ContainingType.IsImplicitlyDeclared)
		{
			Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_CompilerGeneratedAttribute__ctor));
		}
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.SynthesizeDebuggerBrowsableNeverAttribute());
	}

	internal override ConstantValue GetConstantValue(ConstantFieldsInProgress inProgress, bool earlyDecodingWellKnownAttributes)
	{
		return null;
	}
}
