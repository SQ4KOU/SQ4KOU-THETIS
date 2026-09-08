using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat;

internal sealed class SerializationEvents
{
	private static readonly ConcurrentDictionary<Type, SerializationEvents> s_cache = new ConcurrentDictionary<Type, SerializationEvents>();

	private static readonly SerializationEvents s_noEvents = new SerializationEvents();

	private readonly List<MethodInfo> _onDeserializingMethods;

	private readonly List<MethodInfo> _onDeserializedMethods;

	private SerializationEvents()
	{
	}

	private SerializationEvents(List<MethodInfo> onDeserializingMethods, List<MethodInfo> onDeserializedMethods)
	{
		_onDeserializingMethods = onDeserializingMethods;
		_onDeserializedMethods = onDeserializedMethods;
	}

	[UnconditionalSuppressMessage("ReflectionAnalysis", "IL2111:UnrecognizedReflectionPattern", Justification = "The Type is annotated correctly, it just can't pass through the lambda method.")]
	private static SerializationEvents GetSerializationEventsForType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type t)
	{
		return s_cache.GetOrAdd(t, CreateSerializationEvents);
	}

	private static SerializationEvents CreateSerializationEvents([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
	{
		List<MethodInfo> methodsWithAttribute = GetMethodsWithAttribute(typeof(OnDeserializingAttribute), type);
		List<MethodInfo> methodsWithAttribute2 = GetMethodsWithAttribute(typeof(OnDeserializedAttribute), type);
		if (methodsWithAttribute != null || methodsWithAttribute2 != null)
		{
			return new SerializationEvents(methodsWithAttribute, methodsWithAttribute2);
		}
		return s_noEvents;
	}

	private static List<MethodInfo> GetMethodsWithAttribute(Type attribute, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
	{
		List<MethodInfo> list = null;
		Type type2 = type;
		while ((object)type2 != null && type2 != typeof(object))
		{
			MethodInfo[] methods = type2.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.IsDefined(attribute, inherit: false))
				{
					if (list == null)
					{
						list = new List<MethodInfo>();
					}
					list.Add(methodInfo);
				}
			}
			type2 = type2.BaseType;
		}
		list?.Reverse();
		return list;
	}

	internal static Action<StreamingContext> GetOnDeserializingForType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, object obj)
	{
		return GetSerializationEventsForType(type).GetOnDeserializing(obj);
	}

	internal static Action<StreamingContext> GetOnDeserializedForType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type, object obj)
	{
		return GetSerializationEventsForType(type).GetOnDeserialized(obj);
	}

	private Action<StreamingContext> GetOnDeserialized(object obj)
	{
		return AddOnDelegate(obj, _onDeserializedMethods);
	}

	private Action<StreamingContext> GetOnDeserializing(object obj)
	{
		return AddOnDelegate(obj, _onDeserializingMethods);
	}

	private static Action<StreamingContext> AddOnDelegate(object obj, List<MethodInfo> methods)
	{
		Action<StreamingContext> action = null;
		if (methods != null)
		{
			foreach (MethodInfo method in methods)
			{
				Action<StreamingContext> b = (Action<StreamingContext>)method.CreateDelegate(typeof(Action<StreamingContext>), obj);
				action = (Action<StreamingContext>)Delegate.Combine(action, b);
			}
		}
		return action;
	}
}
