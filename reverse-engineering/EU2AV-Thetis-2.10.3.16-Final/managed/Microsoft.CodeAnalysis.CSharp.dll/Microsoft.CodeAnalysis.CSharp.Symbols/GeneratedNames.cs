using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class GeneratedNames
{
	internal const string AnonymousTypeNameWithoutModulePrefix = "<>f__AnonymousType";

	internal const string AnonymousDelegateNameWithoutModulePrefix = "<>f__AnonymousDelegate";

	internal const string ActionDelegateNamePrefix = "<>A";

	internal const string FuncDelegateNamePrefix = "<>F";

	private const int DelegateNamePrefixLength = 3;

	private const int DelegateNamePrefixLengthWithOpenBrace = 4;

	internal static bool IsGeneratedMemberName(string memberName)
	{
		if (memberName.Length > 0)
		{
			return memberName[0] == '<';
		}
		return false;
	}

	internal static string MakeBackingFieldName(string propertyName)
	{
		return "<" + propertyName + ">k__BackingField";
	}

	internal static string MakePrimaryConstructorParameterFieldName(string parameterName)
	{
		return "<" + parameterName + ">P";
	}

	internal static string MakeIteratorFinallyMethodName(StateMachineState finalizeState)
	{
		return "<>m__Finally" + StringExtensions.GetNumeral(0 - (finalizeState + 2));
	}

	internal static string MakeStaticLambdaDisplayClassName(int methodOrdinal, int generation)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.LambdaDisplayClass, methodOrdinal, generation);
	}

	internal static string MakeLambdaDisplayClassName(int methodOrdinal, int generation, int closureOrdinal, int closureGeneration)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.LambdaDisplayClass, methodOrdinal, generation, null, "DisplayClass", '\0', closureOrdinal, closureGeneration);
	}

	internal static string MakeAnonymousTypeOrDelegateTemplateName(int index, int submissionSlotIndex, string moduleId, bool isDelegate)
	{
		string text = "<" + moduleId + (isDelegate ? ">f__AnonymousDelegate" : ">f__AnonymousType") + StringExtensions.GetNumeral(index);
		if (submissionSlotIndex >= 0)
		{
			text = text + "#" + StringExtensions.GetNumeral(submissionSlotIndex);
		}
		return text;
	}

	internal static string MakeAnonymousTypeBackingFieldName(string propertyName)
	{
		return "<" + propertyName + ">i__Field";
	}

	internal static string MakeExtensionName(int index)
	{
		return "<>E__" + StringExtensions.GetNumeral(index);
	}

	internal static string MakeAnonymousTypeParameterName(string propertyName)
	{
		return "<" + propertyName + ">j__TPar";
	}

	internal static string MakeStateMachineTypeName(string methodName, int methodOrdinal, int generation)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.StateMachineType, methodOrdinal, generation, methodName);
	}

	internal static string MakeBaseMethodWrapperName(int uniqueId)
	{
		return "<>n__" + StringExtensions.GetNumeral(uniqueId);
	}

	internal static string MakeLambdaMethodName(string methodName, int methodOrdinal, int methodGeneration, int lambdaOrdinal, int lambdaGeneration)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.LambdaMethod, methodOrdinal, methodGeneration, methodName, null, '\0', lambdaOrdinal, lambdaGeneration);
	}

	internal static string MakeLambdaCacheFieldName(int methodOrdinal, int generation, int lambdaOrdinal, int lambdaGeneration)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.LambdaCacheField, methodOrdinal, generation, null, null, '\0', lambdaOrdinal, lambdaGeneration);
	}

	internal static string MakeLocalFunctionName(string methodName, string localFunctionName, int methodOrdinal, int methodGeneration, int lambdaOrdinal, int lambdaGeneration)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.LocalFunction, methodOrdinal, methodGeneration, methodName, localFunctionName, '|', lambdaOrdinal, lambdaGeneration);
	}

	private static string MakeMethodScopedSynthesizedName(GeneratedNameKind kind, int methodOrdinal, int methodGeneration, string? methodName = null, string? suffix = null, char suffixTerminator = '\0', int entityOrdinal = -1, int entityGeneration = -1)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append('<');
		if (methodName != null)
		{
			builder.Append(methodName);
			if (kind.IsTypeName())
			{
				builder.Replace('.', '-');
			}
		}
		builder.Append('>');
		builder.Append((char)kind);
		if (suffix != null || methodOrdinal >= 0 || entityOrdinal >= 0)
		{
			builder.Append("__");
			builder.Append(suffix);
			if (suffixTerminator != 0)
			{
				builder.Append(suffixTerminator);
			}
			if (methodOrdinal >= 0)
			{
				builder.Append(methodOrdinal.ToString(CultureInfo.InvariantCulture));
				AppendOptionalGeneration(builder, methodGeneration);
			}
			if (entityOrdinal >= 0)
			{
				if (methodOrdinal >= 0)
				{
					builder.Append('_');
				}
				builder.Append(entityOrdinal.ToString(CultureInfo.InvariantCulture));
				AppendOptionalGeneration(builder, entityGeneration);
			}
		}
		return instance.ToStringAndFree();
	}

	private static void AppendOptionalGeneration(StringBuilder builder, int generation)
	{
		if (generation > 0)
		{
			builder.Append('#');
			builder.Append(generation.ToString(CultureInfo.InvariantCulture));
		}
	}

	internal static string MakeHoistedLocalFieldName(SynthesizedLocalKind kind, int slotIndex, string? localName = null)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append('<');
		if (localName != null)
		{
			builder.Append(localName);
		}
		builder.Append('>');
		switch (kind)
		{
		case SynthesizedLocalKind.LambdaDisplayClass:
			builder.Append('8');
			break;
		case SynthesizedLocalKind.UserDefined:
			builder.Append('5');
			break;
		default:
			builder.Append('s');
			break;
		}
		builder.Append("__");
		builder.Append((slotIndex + 1).ToString(CultureInfo.InvariantCulture));
		return instance.ToStringAndFree();
	}

	internal static string AsyncAwaiterFieldName(int slotIndex)
	{
		return "<>u__" + StringExtensions.GetNumeral(slotIndex + 1);
	}

	internal static string MakeCachedFrameInstanceFieldName()
	{
		return "<>9";
	}

	internal static string? MakeSynthesizedLocalName(SynthesizedLocalKind kind, ref int uniqueId)
	{
		if (kind == SynthesizedLocalKind.LambdaDisplayClass)
		{
			return MakeLambdaDisplayLocalName(uniqueId++);
		}
		return null;
	}

	internal static string MakeSynthesizedInstrumentationPayloadLocalFieldName(int uniqueId)
	{
		return "CS$InstrumentationPayload" + StringExtensions.GetNumeral(uniqueId);
	}

	internal static string MakeLambdaDisplayLocalName(int uniqueId)
	{
		return "CS$<>8__locals" + StringExtensions.GetNumeral(uniqueId);
	}

	internal static string MakeFixedFieldImplementationName(string fieldName)
	{
		return "<" + fieldName + ">e__FixedBuffer";
	}

	internal static string MakeStateMachineStateFieldName()
	{
		return "<>1__state";
	}

	internal static string MakeAsyncIteratorPromiseOfValueOrEndFieldName()
	{
		return "<>v__promiseOfValueOrEnd";
	}

	internal static string MakeAsyncIteratorCombinedTokensFieldName()
	{
		return "<>x__combinedTokens";
	}

	internal static string MakeIteratorCurrentFieldName()
	{
		return "<>2__current";
	}

	internal static string MakeDisposeModeFieldName()
	{
		return "<>w__disposeMode";
	}

	internal static string MakeIteratorCurrentThreadIdFieldName()
	{
		return "<>l__initialThreadId";
	}

	internal static string MakeStateMachineStateIdFieldName()
	{
		return "<>I";
	}

	internal static string ThisProxyFieldName()
	{
		return "<>4__this";
	}

	internal static string StateMachineThisParameterProxyName()
	{
		return StateMachineParameterProxyFieldName(ThisProxyFieldName());
	}

	internal static string StateMachineParameterProxyFieldName(string parameterName)
	{
		return "<>3__" + parameterName;
	}

	internal static string MakeDynamicCallSiteContainerName(int methodOrdinal, int localFunctionOrdinal, int generation)
	{
		return MakeMethodScopedSynthesizedName(GeneratedNameKind.DynamicCallSiteContainerType, methodOrdinal, generation, null, (localFunctionOrdinal != -1) ? localFunctionOrdinal.ToString() : null, (localFunctionOrdinal != -1) ? '|' : '\0');
	}

	internal static string MakeDynamicCallSiteFieldName(int uniqueId)
	{
		return "<>p__" + StringExtensions.GetNumeral(uniqueId);
	}

	internal static string MakeSynthesizedDelegateName(RefKindVector byRefs, bool returnsVoid, int generation)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append(returnsVoid ? "<>A" : "<>F");
		if (!byRefs.IsNull)
		{
			builder.Append(byRefs.ToRefKindString());
		}
		AppendOptionalGeneration(builder, generation);
		return instance.ToStringAndFree();
	}

	internal static bool TryParseSynthesizedDelegateName(string name, out RefKindVector byRefs, out bool returnsVoid, out int generation, out int parameterCount)
	{
		byRefs = default(RefKindVector);
		parameterCount = 0;
		generation = 0;
		name = MetadataHelpers.InferTypeArityAndUnmangleMetadataName(name, out var arity);
		returnsVoid = name.StartsWith("<>A");
		if (!returnsVoid && !name.StartsWith("<>F"))
		{
			return false;
		}
		parameterCount = (int)arity - ((!returnsVoid) ? 1 : 0);
		int num = name.LastIndexOf('}');
		if (num < 0)
		{
			num = 2;
		}
		else
		{
			if (name.Length <= 3 || name[3] != '{')
			{
				return false;
			}
			if (!RefKindVector.TryParse(name.Substring(4, num - 4), arity, out byRefs))
			{
				return false;
			}
		}
		if (num < name.Length - 1)
		{
			if (name[num + 1] != '#')
			{
				return false;
			}
			string text = name;
			int num2 = num + 2;
			if (!int.TryParse(text.Substring(num2, text.Length - num2), out generation))
			{
				return false;
			}
		}
		return true;
	}

	internal static string MakeSynthesizedInlineArrayName(int arrayLength, int generation)
	{
		string text = "<>y__InlineArray" + arrayLength;
		if (generation <= 0)
		{
			return text;
		}
		return text + "#" + generation;
	}

	internal static string MakeSynthesizedReadOnlyListName(SynthesizedReadOnlyListKind kind, int generation)
	{
		string text = kind switch
		{
			SynthesizedReadOnlyListKind.Array => "<>z__ReadOnlyArray", 
			SynthesizedReadOnlyListKind.List => "<>z__ReadOnlyList", 
			SynthesizedReadOnlyListKind.SingleElement => "<>z__ReadOnlySingleElementList", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
		if (generation <= 0)
		{
			return text;
		}
		return text + "#" + generation;
	}

	internal static string AsyncBuilderFieldName()
	{
		return "<>t__builder";
	}

	internal static string DelegateCacheContainerType(int generation, string? methodName = null, int methodOrdinal = -1, int ownerUniqueId = -1)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append('<').Append(methodName).Append('>')
			.Append('O');
		if (methodOrdinal > -1)
		{
			builder.Append("__").Append(methodOrdinal.ToString(CultureInfo.InvariantCulture));
		}
		if (ownerUniqueId > -1)
		{
			builder.Append('_').Append(ownerUniqueId.ToString(CultureInfo.InvariantCulture));
		}
		AppendOptionalGeneration(builder, generation);
		return instance.ToStringAndFree();
	}

	internal static string DelegateCacheContainerFieldName(int id, string targetMethod)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		instance.Builder.Append('<').Append(id.ToString(CultureInfo.InvariantCulture)).Append(">__")
			.Append(targetMethod);
		return instance.ToStringAndFree();
	}

	internal static string ReusableHoistedLocalFieldName(int number)
	{
		return "<>7__wrap" + StringExtensions.GetNumeral(number);
	}

	internal static string LambdaCopyParameterName(int ordinal)
	{
		return "<p" + StringExtensions.GetNumeral(ordinal) + ">";
	}

	internal static string AnonymousDelegateParameterName(int index, int parameterCount)
	{
		if (parameterCount == 1)
		{
			return "arg";
		}
		return "arg" + StringExtensions.GetNumeral(index + 1);
	}

	internal static string MakeFileTypeMetadataNamePrefix(string filePath, ImmutableArray<byte> checksumOpt)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		StringBuilder builder = instance.Builder;
		builder.Append('<');
		AppendFileName(filePath, builder);
		builder.Append('>');
		builder.Append('F');
		if (checksumOpt.IsDefault)
		{
			builder.Append("<no checksum>");
		}
		else
		{
			foreach (byte item in checksumOpt)
			{
				builder.AppendFormat("{0:X2}", item);
			}
		}
		builder.Append("__");
		return instance.ToStringAndFree();
	}

	internal static string GetDisplayFilePath(string filePath)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		AppendFileName(filePath, instance.Builder);
		return instance.ToStringAndFree();
	}

	private static void AppendFileName(string filePath, StringBuilder sb)
	{
		string fileName = FileNameUtilities.GetFileName(filePath, includeExtension: false);
		if (fileName == null)
		{
			return;
		}
		string text = fileName;
		foreach (char c in text)
		{
			char value;
			switch (c)
			{
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				value = c;
				break;
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
			case 'G':
			case 'H':
			case 'I':
			case 'J':
			case 'K':
			case 'L':
			case 'M':
			case 'N':
			case 'O':
			case 'P':
			case 'Q':
			case 'R':
			case 'S':
			case 'T':
			case 'U':
			case 'V':
			case 'W':
			case 'X':
			case 'Y':
			case 'Z':
				value = c;
				break;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				value = c;
				break;
			default:
				value = '_';
				break;
			}
			sb.Append(value);
		}
	}
}
