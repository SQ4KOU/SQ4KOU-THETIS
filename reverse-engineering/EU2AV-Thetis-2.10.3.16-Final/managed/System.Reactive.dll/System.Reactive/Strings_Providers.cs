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
internal class Strings_Providers
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
				resourceMan = new ResourceManager("System.Reactive.Strings_Providers", typeof(Strings_Providers).GetTypeInfo().Assembly);
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

	internal static string EXPECTED_TOQUERYABLE_METHODCALL => ResourceManager.GetString("EXPECTED_TOQUERYABLE_METHODCALL", resourceCulture);

	internal static string INVALID_TREE_TYPE => ResourceManager.GetString("INVALID_TREE_TYPE", resourceCulture);

	internal static string NO_MATCHING_METHOD_FOUND => ResourceManager.GetString("NO_MATCHING_METHOD_FOUND", resourceCulture);

	internal Strings_Providers()
	{
	}
}
