using System;

namespace WindowsFirewallHelper;

public class FirewallWASInternetControlMessage : IEquatable<FirewallWASInternetControlMessage>
{
	public static readonly FirewallWASInternetControlMessage Any = new FirewallWASInternetControlMessage();

	private readonly byte? _code;

	private readonly byte? _type;

	public int Code => ((int?)_code) ?? (-1);

	public int Type => ((int?)_type) ?? (-1);

	public FirewallWASInternetControlMessage(byte type)
	{
		_type = type;
		_code = null;
	}

	public FirewallWASInternetControlMessage(byte type, byte code)
	{
		_type = type;
		_code = code;
	}

	public FirewallWASInternetControlMessage(InternetControlMessageKnownTypesV6 type)
		: this((byte)type)
	{
	}

	public FirewallWASInternetControlMessage(InternetControlMessageKnownTypesV6 type, byte code)
		: this((byte)type, code)
	{
	}

	public FirewallWASInternetControlMessage(InternetControlMessageKnownTypes type)
		: this((byte)type)
	{
	}

	public FirewallWASInternetControlMessage(InternetControlMessageKnownTypes type, byte code)
		: this((byte)type, code)
	{
	}

	private FirewallWASInternetControlMessage()
	{
		_type = null;
		_code = null;
	}

	public bool Equals(FirewallWASInternetControlMessage other)
	{
		if (other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (Type == other.Type)
		{
			return Code == other.Code;
		}
		return false;
	}

	public static bool operator ==(FirewallWASInternetControlMessage left, FirewallWASInternetControlMessage right)
	{
		if (!object.Equals(left, right))
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FirewallWASInternetControlMessage left, FirewallWASInternetControlMessage right)
	{
		return !(left == right);
	}

	public static bool TryParse(string str, out FirewallWASInternetControlMessage icm)
	{
		string[] array = str.Split(':');
		if (array.Length == 1)
		{
			if (array[0].Trim() == "*")
			{
				icm = Any;
				return true;
			}
		}
		else if (array.Length == 2)
		{
			if (array[0].Trim() == "*" && array[1].Trim() == "*")
			{
				icm = Any;
				return true;
			}
			if (byte.TryParse(array[0].Trim(), out var result))
			{
				if (array[1].Trim() == "*")
				{
					icm = new FirewallWASInternetControlMessage(result);
					return true;
				}
				if (byte.TryParse(array[1].Trim(), out var result2))
				{
					icm = new FirewallWASInternetControlMessage(result, result2);
					return true;
				}
			}
		}
		icm = null;
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FirewallWASInternetControlMessage);
	}

	public override int GetHashCode()
	{
		return (Type.GetHashCode() * 397) ^ Code.GetHashCode();
	}

	public override string ToString()
	{
		if (Equals(Any))
		{
			return "*";
		}
		return string.Format("{0}:{1}", _type, _code?.ToString() ?? "*");
	}
}
