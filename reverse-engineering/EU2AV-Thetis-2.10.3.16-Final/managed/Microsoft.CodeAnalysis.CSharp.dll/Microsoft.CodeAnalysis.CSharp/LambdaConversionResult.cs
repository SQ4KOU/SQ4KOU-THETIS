namespace Microsoft.CodeAnalysis.CSharp;

internal enum LambdaConversionResult
{
	Success,
	BadTargetType,
	BadParameterCount,
	MissingSignatureWithOutParameter,
	MismatchedReturnType,
	MismatchedParameterType,
	MismatchedParameterRefKind,
	StaticTypeInImplicitlyTypedLambda,
	ExpressionTreeMustHaveDelegateTypeArgument,
	ExpressionTreeFromAnonymousMethod,
	BindingFailed
}
