using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class TypeConversions : ConversionsBase
{
	protected override CSharpCompilation Compilation => null;

	protected override bool IsAttributeArgumentBinding => false;

	protected override bool IsParameterDefaultValueBinding => false;

	public TypeConversions(AssemblySymbol corLibrary, bool includeNullability = false)
		: this(corLibrary, 0, includeNullability, null)
	{
	}

	private TypeConversions(AssemblySymbol corLibrary, int currentRecursionDepth, bool includeNullability, TypeConversions otherNullabilityOpt)
		: base(corLibrary, currentRecursionDepth, includeNullability, otherNullabilityOpt)
	{
	}

	protected override ConversionsBase CreateInstance(int currentRecursionDepth)
	{
		return new TypeConversions(corLibrary, currentRecursionDepth, IncludeNullability, null);
	}

	protected override ConversionsBase WithNullabilityCore(bool includeNullability)
	{
		return new TypeConversions(corLibrary, currentRecursionDepth, includeNullability, this);
	}

	public override Conversion GetMethodGroupDelegateConversion(BoundMethodGroup source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Conversions/TypeConversions.cs", 39);
	}

	public override Conversion GetMethodGroupFunctionPointerConversion(BoundMethodGroup source, FunctionPointerTypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Conversions/TypeConversions.cs", 45);
	}

	public override Conversion GetStackAllocConversion(BoundStackAllocArrayCreation sourceExpression, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Conversions/TypeConversions.cs", 51);
	}

	protected override Conversion GetInterpolatedStringConversion(BoundExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Conversions/TypeConversions.cs", 57);
	}

	protected override Conversion GetCollectionExpressionConversion(BoundUnconvertedCollectionExpression source, TypeSymbol destination, ref CompoundUseSiteInfo<AssemblySymbol> useSiteInfo)
	{
		throw ExceptionUtilities.Unreachable("/_/src/Compilers/CSharp/Portable/Binder/Semantics/Conversions/TypeConversions.cs", 63);
	}
}
