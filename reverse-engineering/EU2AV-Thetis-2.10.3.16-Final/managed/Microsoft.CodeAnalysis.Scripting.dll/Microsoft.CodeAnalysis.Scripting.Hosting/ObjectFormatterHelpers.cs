using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal static class ObjectFormatterHelpers
{
	internal static readonly object VoidValue = new object();

	internal const int NumberRadixDecimal = 10;

	internal const int NumberRadixHexadecimal = 16;

	internal static bool HasOverriddenToString(System.Reflection.TypeInfo type)
	{
		if (type.IsInterface)
		{
			return false;
		}
		while (type.AsType() != typeof(object))
		{
			if (type.GetDeclaredMethod("ToString", Type.EmptyTypes) != null)
			{
				return true;
			}
			type = type.BaseType.GetTypeInfo();
		}
		return false;
	}

	internal static DebuggerDisplayAttribute GetApplicableDebuggerDisplayAttribute(MemberInfo member)
	{
		DebuggerDisplayAttribute debuggerDisplayAttribute = member.GetCustomAttributes<DebuggerDisplayAttribute>().FirstOrDefault();
		if (debuggerDisplayAttribute != null)
		{
			return debuggerDisplayAttribute;
		}
		if (member is System.Reflection.TypeInfo typeInfo)
		{
			foreach (DebuggerDisplayAttribute customAttribute in typeInfo.Assembly.GetCustomAttributes<DebuggerDisplayAttribute>())
			{
				if (IsApplicableAttribute(typeInfo, customAttribute.Target.GetTypeInfo(), customAttribute.TargetTypeName))
				{
					return customAttribute;
				}
			}
		}
		return null;
	}

	private static DebuggerTypeProxyAttribute GetApplicableDebuggerTypeProxyAttribute(System.Reflection.TypeInfo type)
	{
		DebuggerTypeProxyAttribute debuggerTypeProxyAttribute = type.GetCustomAttributes<DebuggerTypeProxyAttribute>().FirstOrDefault();
		if (debuggerTypeProxyAttribute != null)
		{
			return debuggerTypeProxyAttribute;
		}
		foreach (DebuggerTypeProxyAttribute customAttribute in type.Assembly.GetCustomAttributes<DebuggerTypeProxyAttribute>())
		{
			if (IsApplicableAttribute(type, customAttribute.Target.GetTypeInfo(), customAttribute.TargetTypeName))
			{
				return customAttribute;
			}
		}
		return null;
	}

	private static bool IsApplicableAttribute(System.Reflection.TypeInfo type, System.Reflection.TypeInfo targetType, string targetTypeName)
	{
		if (!(type != null) || !AreEquivalent(targetType, type))
		{
			if (targetTypeName != null)
			{
				return type.FullName == targetTypeName;
			}
			return false;
		}
		return true;
	}

	private static bool AreEquivalent(System.Reflection.TypeInfo type, System.Reflection.TypeInfo other)
	{
		return type.Equals(other);
	}

	internal static object GetDebuggerTypeProxy(object obj)
	{
		System.Reflection.TypeInfo typeInfo = obj.GetType().GetTypeInfo();
		DebuggerTypeProxyAttribute applicableDebuggerTypeProxyAttribute = GetApplicableDebuggerTypeProxyAttribute(typeInfo);
		if (applicableDebuggerTypeProxyAttribute != null)
		{
			try
			{
				Type type = Type.GetType(applicableDebuggerTypeProxyAttribute.ProxyTypeName, throwOnError: false, ignoreCase: false);
				if (type != null)
				{
					if (type.GetTypeInfo().IsGenericTypeDefinition)
					{
						type = type.MakeGenericType(typeInfo.GenericTypeArguments);
					}
					return Activator.CreateInstance(type, obj);
				}
			}
			catch (Exception)
			{
			}
		}
		return null;
	}

	internal static MemberInfo ResolveMember(object obj, string memberName, bool callableOnly)
	{
		System.Reflection.TypeInfo typeInfo = obj.GetType().GetTypeInfo();
		System.Reflection.TypeInfo typeInfo2 = typeInfo;
		while (true)
		{
			if (!callableOnly)
			{
				FieldInfo declaredField = typeInfo2.GetDeclaredField(memberName);
				if (declaredField != null)
				{
					return declaredField;
				}
				PropertyInfo declaredProperty = typeInfo2.GetDeclaredProperty(memberName);
				if (declaredProperty != null)
				{
					MethodInfo getMethod = declaredProperty.GetMethod;
					if (getMethod != null)
					{
						return getMethod;
					}
				}
			}
			MethodInfo declaredMethod = typeInfo2.GetDeclaredMethod(memberName, Type.EmptyTypes);
			if (declaredMethod != null)
			{
				return declaredMethod;
			}
			if (typeInfo2.BaseType == null)
			{
				break;
			}
			typeInfo2 = typeInfo2.BaseType.GetTypeInfo();
		}
		typeInfo2 = typeInfo;
		while (true)
		{
			IEnumerable<MemberInfo> enumerable = ((!callableOnly) ? ((IEnumerable<MemberInfo>)typeInfo.DeclaredFields).Concat((IEnumerable<MemberInfo>)typeInfo.DeclaredProperties) : typeInfo.DeclaredMethods);
			MemberInfo memberInfo = null;
			foreach (MemberInfo item in enumerable)
			{
				if (!StringComparer.OrdinalIgnoreCase.Equals(memberName, item.Name))
				{
					continue;
				}
				if (memberInfo != null)
				{
					return null;
				}
				if (item is FieldInfo)
				{
					memberInfo = item;
					continue;
				}
				MethodInfo methodInfo;
				if ((methodInfo = item as MethodInfo) != null)
				{
					if (methodInfo.GetParameters().Length == 0)
					{
						memberInfo = item;
					}
					continue;
				}
				MethodInfo? getMethod2 = ((PropertyInfo)item).GetMethod;
				if ((object)getMethod2 != null && getMethod2.GetParameters().Length == 0)
				{
					memberInfo = item;
				}
			}
			if (memberInfo != null)
			{
				return memberInfo;
			}
			if (typeInfo2.BaseType == null)
			{
				break;
			}
			typeInfo2 = typeInfo2.BaseType.GetTypeInfo();
		}
		return null;
	}

	internal static object GetMemberValue(MemberInfo member, object obj, out Exception exception)
	{
		exception = null;
		try
		{
			FieldInfo fieldInfo;
			if ((fieldInfo = member as FieldInfo) != null)
			{
				return fieldInfo.GetValue(obj);
			}
			MethodInfo methodInfo;
			if ((methodInfo = member as MethodInfo) != null)
			{
				return (methodInfo.ReturnType == typeof(void)) ? VoidValue : methodInfo.Invoke(obj, Array.Empty<object>());
			}
			PropertyInfo propertyInfo = (PropertyInfo)member;
			if (propertyInfo.GetMethod == null)
			{
				return null;
			}
			return propertyInfo.GetValue(obj, Array.Empty<object>());
		}
		catch (TargetInvocationException ex)
		{
			exception = ex.InnerException;
		}
		return null;
	}

	internal static SpecialType GetPrimitiveSpecialType(Type type)
	{
		if (type == typeof(int))
		{
			return SpecialType.System_Int32;
		}
		if (type == typeof(string))
		{
			return SpecialType.System_String;
		}
		if (type == typeof(bool))
		{
			return SpecialType.System_Boolean;
		}
		if (type == typeof(char))
		{
			return SpecialType.System_Char;
		}
		if (type == typeof(long))
		{
			return SpecialType.System_Int64;
		}
		if (type == typeof(double))
		{
			return SpecialType.System_Double;
		}
		if (type == typeof(byte))
		{
			return SpecialType.System_Byte;
		}
		if (type == typeof(decimal))
		{
			return SpecialType.System_Decimal;
		}
		if (type == typeof(uint))
		{
			return SpecialType.System_UInt32;
		}
		if (type == typeof(ulong))
		{
			return SpecialType.System_UInt64;
		}
		if (type == typeof(float))
		{
			return SpecialType.System_Single;
		}
		if (type == typeof(short))
		{
			return SpecialType.System_Int16;
		}
		if (type == typeof(ushort))
		{
			return SpecialType.System_UInt16;
		}
		if (type == typeof(DateTime))
		{
			return SpecialType.System_DateTime;
		}
		if (type == typeof(sbyte))
		{
			return SpecialType.System_SByte;
		}
		if (type == typeof(object))
		{
			return SpecialType.System_Object;
		}
		if (type == typeof(void))
		{
			return SpecialType.System_Void;
		}
		return SpecialType.None;
	}

	internal static ObjectDisplayOptions GetObjectDisplayOptions(bool useQuotes = false, bool escapeNonPrintable = false, bool includeCodePoints = false, int numberRadix = 10)
	{
		ObjectDisplayOptions objectDisplayOptions = ObjectDisplayOptions.None;
		if (useQuotes)
		{
			objectDisplayOptions |= ObjectDisplayOptions.UseQuotes;
		}
		if (escapeNonPrintable)
		{
			objectDisplayOptions |= ObjectDisplayOptions.EscapeNonPrintableCharacters;
		}
		if (includeCodePoints)
		{
			objectDisplayOptions |= ObjectDisplayOptions.IncludeCodePoints;
		}
		switch (numberRadix)
		{
		case 16:
			objectDisplayOptions |= ObjectDisplayOptions.UseHexadecimalNumbers;
			break;
		default:
			throw new ArgumentNullException("numberRadix");
		case 10:
			break;
		}
		return objectDisplayOptions;
	}

	internal static string ParseSimpleMemberName(string str, int start, int end, out bool noQuotes, out bool isCallable)
	{
		isCallable = false;
		noQuotes = false;
		if (end - 3 >= start && str[end - 2] == 'n' && str[end - 1] == 'q')
		{
			int num = end - 3;
			while (num >= start && char.IsWhiteSpace(str[num]))
			{
				num--;
			}
			if (num >= start && str[num] == ',')
			{
				noQuotes = true;
				end = num;
			}
		}
		int i = end - 1;
		EatTrailingWhiteSpace(str, start, ref i);
		if (i > start && str[i] == ')')
		{
			int num2 = i;
			i--;
			EatTrailingWhiteSpace(str, start, ref i);
			if (str[i] != '(')
			{
				i = num2;
			}
			else
			{
				i--;
				EatTrailingWhiteSpace(str, start, ref i);
				isCallable = true;
			}
		}
		EatLeadingWhiteSpace(str, ref start, i);
		return str.Substring(start, i - start + 1);
	}

	private static void EatTrailingWhiteSpace(string str, int start, ref int i)
	{
		while (i >= start && char.IsWhiteSpace(str[i]))
		{
			i--;
		}
	}

	private static void EatLeadingWhiteSpace(string str, ref int i, int end)
	{
		while (i < end && char.IsWhiteSpace(str[i]))
		{
			i++;
		}
	}
}
