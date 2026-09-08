using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Resources.Extensions.BinaryFormat;

internal static class SerializationInfoExtensions
{
	private static readonly Action<SerializationInfo, string, object, Type> s_updateValue = typeof(SerializationInfo).GetMethod("UpdateValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).CreateDelegate(typeof(Action<SerializationInfo, string, object, Type>)) as Action<SerializationInfo, string, object, Type>;

	[DynamicDependency(DynamicallyAccessedMemberTypes.NonPublicMethods, typeof(SerializationInfo))]
	internal static void UpdateValue(this SerializationInfo si, string name, object value, Type type)
	{
		s_updateValue(si, name, value, type);
	}
}
