using System;
using System.Runtime.InteropServices;
using WindowsFirewallHelper.InternalHelpers;

namespace WindowsFirewallHelper;

public class COMTypeResolver
{
	public string MachineName { get; }

	public COMTypeResolver(string machineName)
	{
		MachineName = machineName;
	}

	public COMTypeResolver()
		: this(null)
	{
	}

	internal T CreateInstance<T>()
	{
		if (!typeof(T).IsInterface)
		{
			throw new ArgumentException("Invalid generic type passed.", "T");
		}
		try
		{
			string classProgId = ComClassProgIdAttribute.GetClassProgId<T>();
			if (!string.IsNullOrWhiteSpace(classProgId))
			{
				Type typeFromProgID = Type.GetTypeFromProgID(classProgId, MachineName, throwOnError: false);
				if (typeFromProgID != null)
				{
					try
					{
						return (T)Activator.CreateInstance(typeFromProgID);
					}
					catch (COMException)
					{
						if (MachineName == null)
						{
							throw;
						}
					}
				}
			}
			Type typeFromCLSID = Type.GetTypeFromCLSID(typeof(T).GUID, MachineName, throwOnError: false);
			if (typeFromCLSID != null)
			{
				try
				{
					return (T)Activator.CreateInstance(typeFromCLSID);
				}
				catch (COMException)
				{
					if (MachineName == null)
					{
						throw;
					}
				}
			}
		}
		catch (COMException innerException)
		{
			throw new NotSupportedException("Can not create a new instance of this interface in current environment.", innerException);
		}
		throw new NotSupportedException("Can not create a new instance of this interface in current environment.");
	}

	internal bool IsSupported<T>()
	{
		if (!typeof(T).IsInterface)
		{
			throw new ArgumentException("Invalid generic type passed.", "T");
		}
		try
		{
			return CreateInstance<T>() != null;
		}
		catch
		{
			return false;
		}
	}
}
