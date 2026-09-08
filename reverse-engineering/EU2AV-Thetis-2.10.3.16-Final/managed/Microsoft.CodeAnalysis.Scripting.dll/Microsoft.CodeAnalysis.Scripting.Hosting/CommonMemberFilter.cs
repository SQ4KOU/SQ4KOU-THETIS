using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal class CommonMemberFilter : MemberFilter
{
	public override bool Include(StackFrame frame)
	{
		MethodBase method = frame.GetMethod();
		Type? declaringType = method.DeclaringType;
		if ((object)declaringType != null && declaringType.FullName.StartsWith("Submission#0").ToThreeState() == ThreeState.True)
		{
			return true;
		}
		if (IsHiddenMember(method))
		{
			return false;
		}
		Type declaringType2 = method.DeclaringType;
		if (declaringType2 == typeof(CommandLineRunner))
		{
			return false;
		}
		if (declaringType2 == null || IsTaskAwaiter(declaringType2) || IsTaskAwaiter(declaringType2.DeclaringType))
		{
			return false;
		}
		if (declaringType2 == typeof(ExceptionDispatchInfo) && method.Name == "Throw")
		{
			return false;
		}
		return true;
	}

	public override bool Include(MemberInfo member)
	{
		return !IsGeneratedMemberName(member.Name);
	}

	private bool IsHiddenMember(MemberInfo info)
	{
		while (info != null)
		{
			if (!IsGeneratedMemberName(info.Name))
			{
				IEnumerable<DebuggerHiddenAttribute> customAttributes = info.GetCustomAttributes<DebuggerHiddenAttribute>();
				if (customAttributes == null || !customAttributes.Any())
				{
					info = info.DeclaringType?.GetTypeInfo();
					continue;
				}
			}
			return true;
		}
		return false;
	}

	private static bool IsTaskAwaiter(Type type)
	{
		if (type == typeof(TaskAwaiter) || type == typeof(ConfiguredTaskAwaitable))
		{
			return true;
		}
		if ((object)type != null && type.GetTypeInfo().IsGenericType)
		{
			if (!(type.GetTypeInfo().GetGenericTypeDefinition() == typeof(TaskAwaiter<>)))
			{
				return type == typeof(ConfiguredTaskAwaitable<>);
			}
			return true;
		}
		return false;
	}

	protected virtual bool IsGeneratedMemberName(string name)
	{
		return false;
	}
}
