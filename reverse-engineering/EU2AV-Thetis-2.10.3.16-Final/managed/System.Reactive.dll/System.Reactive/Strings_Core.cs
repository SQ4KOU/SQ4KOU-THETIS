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
internal class Strings_Core
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
				resourceMan = new ResourceManager("System.Reactive.Strings_Core", typeof(Strings_Core).GetTypeInfo().Assembly);
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

	internal static string CANT_OBTAIN_SCHEDULER => ResourceManager.GetString("CANT_OBTAIN_SCHEDULER", resourceCulture);

	internal static string COMPLETED_NO_VALUE => ResourceManager.GetString("COMPLETED_NO_VALUE", resourceCulture);

	internal static string DISPOSABLE_ALREADY_ASSIGNED => ResourceManager.GetString("DISPOSABLE_ALREADY_ASSIGNED", resourceCulture);

	internal static string DISPOSABLES_CANT_CONTAIN_NULL => ResourceManager.GetString("DISPOSABLES_CANT_CONTAIN_NULL", resourceCulture);

	internal static string FAILED_CLOCK_MONITORING => ResourceManager.GetString("FAILED_CLOCK_MONITORING", resourceCulture);

	internal static string HEAP_EMPTY => ResourceManager.GetString("HEAP_EMPTY", resourceCulture);

	internal static string OBSERVER_TERMINATED => ResourceManager.GetString("OBSERVER_TERMINATED", resourceCulture);

	internal static string REENTRANCY_DETECTED => ResourceManager.GetString("REENTRANCY_DETECTED", resourceCulture);

	internal static string SCHEDULER_OPERATION_ALREADY_AWAITED => ResourceManager.GetString("SCHEDULER_OPERATION_ALREADY_AWAITED", resourceCulture);

	internal Strings_Core()
	{
	}
}
