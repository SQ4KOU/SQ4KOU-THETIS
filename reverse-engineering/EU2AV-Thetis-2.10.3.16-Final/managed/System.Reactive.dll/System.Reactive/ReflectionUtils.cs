using System.Globalization;
using System.Reflection;

namespace System.Reactive;

internal static class ReflectionUtils
{
	public static TDelegate CreateDelegate<TDelegate>(object o, MethodInfo method)
	{
		return (TDelegate)(object)method.CreateDelegate(typeof(TDelegate), o);
	}

	public static Delegate CreateDelegate(Type delegateType, object o, MethodInfo method)
	{
		return method.CreateDelegate(delegateType, o);
	}

	public static void GetEventMethods<TSender, TEventArgs>(Type targetType, object? target, string eventName, out MethodInfo addMethod, out MethodInfo removeMethod, out Type delegateType, out bool isWinRT)
	{
		EventInfo eventInfo;
		if (target == null)
		{
			eventInfo = targetType.GetEvent(eventName, BindingFlags.Static | BindingFlags.Public);
			if (eventInfo == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Linq.COULD_NOT_FIND_STATIC_EVENT, eventName, targetType.FullName));
			}
		}
		else
		{
			eventInfo = targetType.GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);
			if (eventInfo == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Linq.COULD_NOT_FIND_INSTANCE_EVENT, eventName, targetType.FullName));
			}
		}
		addMethod = eventInfo.GetAddMethod() ?? throw new InvalidOperationException(Strings_Linq.EVENT_MISSING_ADD_METHOD);
		removeMethod = eventInfo.GetRemoveMethod() ?? throw new InvalidOperationException(Strings_Linq.EVENT_MISSING_REMOVE_METHOD);
		ParameterInfo[] parameters = addMethod.GetParameters();
		if (parameters.Length != 1)
		{
			throw new InvalidOperationException(Strings_Linq.EVENT_ADD_METHOD_SHOULD_TAKE_ONE_PARAMETER);
		}
		ParameterInfo[] parameters2 = removeMethod.GetParameters();
		if (parameters2.Length != 1)
		{
			throw new InvalidOperationException(Strings_Linq.EVENT_REMOVE_METHOD_SHOULD_TAKE_ONE_PARAMETER);
		}
		isWinRT = false;
		if (IsWinRTEventRegistrationTokenType(addMethod.ReturnType))
		{
			isWinRT = true;
			if (IsWinRTEventRegistrationTokenType(parameters2[0].ParameterType))
			{
				throw new InvalidOperationException(Strings_Linq.EVENT_WINRT_REMOVE_METHOD_SHOULD_TAKE_ERT);
			}
		}
		delegateType = parameters[0].ParameterType;
		MethodInfo method = delegateType.GetMethod("Invoke");
		ParameterInfo[] parameters3 = method.GetParameters();
		if (parameters3.Length != 2)
		{
			throw new InvalidOperationException(Strings_Linq.EVENT_PATTERN_REQUIRES_TWO_PARAMETERS);
		}
		if (!typeof(TSender).IsAssignableFrom(parameters3[0].ParameterType))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Linq.EVENT_SENDER_NOT_ASSIGNABLE, typeof(TSender).FullName));
		}
		if (!typeof(TEventArgs).IsAssignableFrom(parameters3[1].ParameterType))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings_Linq.EVENT_ARGS_NOT_ASSIGNABLE, typeof(TEventArgs).FullName));
		}
		if (method.ReturnType != typeof(void))
		{
			throw new InvalidOperationException(Strings_Linq.EVENT_MUST_RETURN_VOID);
		}
	}

	private static bool IsWinRTEventRegistrationTokenType(Type t)
	{
		if (t.Name == "EventRegistrationToken")
		{
			if (!(t.Namespace == "System.Runtime.InteropServices.WindowsRuntime"))
			{
				return t.Namespace == "WinRT";
			}
			return true;
		}
		return false;
	}
}
