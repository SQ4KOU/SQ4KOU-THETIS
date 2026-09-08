using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;

namespace System.Reactive;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Strings_Linq
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("System.Reactive.Strings_Linq", typeof(Strings_Linq).GetTypeInfo().Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string CANT_ADVANCE_WHILE_RUNNING => ResourceManager.GetString("CANT_ADVANCE_WHILE_RUNNING", resourceCulture);

	internal static string COULD_NOT_FIND_INSTANCE_EVENT => ResourceManager.GetString("COULD_NOT_FIND_INSTANCE_EVENT", resourceCulture);

	internal static string COULD_NOT_FIND_STATIC_EVENT => ResourceManager.GetString("COULD_NOT_FIND_STATIC_EVENT", resourceCulture);

	internal static string EVENT_ADD_METHOD_SHOULD_TAKE_ONE_PARAMETER => ResourceManager.GetString("EVENT_ADD_METHOD_SHOULD_TAKE_ONE_PARAMETER", resourceCulture);

	internal static string EVENT_ARGS_NOT_ASSIGNABLE => ResourceManager.GetString("EVENT_ARGS_NOT_ASSIGNABLE", resourceCulture);

	internal static string EVENT_MISSING_ADD_METHOD => ResourceManager.GetString("EVENT_MISSING_ADD_METHOD", resourceCulture);

	internal static string EVENT_MISSING_REMOVE_METHOD => ResourceManager.GetString("EVENT_MISSING_REMOVE_METHOD", resourceCulture);

	internal static string EVENT_MUST_RETURN_VOID => ResourceManager.GetString("EVENT_MUST_RETURN_VOID", resourceCulture);

	internal static string EVENT_PATTERN_REQUIRES_TWO_PARAMETERS => ResourceManager.GetString("EVENT_PATTERN_REQUIRES_TWO_PARAMETERS", resourceCulture);

	internal static string EVENT_REMOVE_METHOD_SHOULD_TAKE_ONE_PARAMETER => ResourceManager.GetString("EVENT_REMOVE_METHOD_SHOULD_TAKE_ONE_PARAMETER", resourceCulture);

	internal static string EVENT_SENDER_NOT_ASSIGNABLE => ResourceManager.GetString("EVENT_SENDER_NOT_ASSIGNABLE", resourceCulture);

	internal static string EVENT_WINRT_REMOVE_METHOD_SHOULD_TAKE_ERT => ResourceManager.GetString("EVENT_WINRT_REMOVE_METHOD_SHOULD_TAKE_ERT", resourceCulture);

	internal static string MORE_THAN_ONE_ELEMENT => ResourceManager.GetString("MORE_THAN_ONE_ELEMENT", resourceCulture);

	internal static string MORE_THAN_ONE_MATCHING_ELEMENT => ResourceManager.GetString("MORE_THAN_ONE_MATCHING_ELEMENT", resourceCulture);

	internal static string NO_ELEMENTS => ResourceManager.GetString("NO_ELEMENTS", resourceCulture);

	internal static string NO_MATCHING_ELEMENTS => ResourceManager.GetString("NO_MATCHING_ELEMENTS", resourceCulture);

	internal Strings_Linq()
	{
	}
}
