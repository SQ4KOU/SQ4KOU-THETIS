using System;
using System.Collections.Generic;
using System.Text;

namespace ExCSS;

public sealed class Url : IEquatable<Url>
{
	private const string CurrentDirectory = ".";

	private const string CurrentDirectoryAlternative = "%2e";

	private const string UpperDirectory = "..";

	private static readonly string[] UpperDirectoryAlternatives = new string[3] { "%2e%2e", ".%2e", "%2e." };

	private static readonly Url DefaultBase = new Url(string.Empty, string.Empty, string.Empty);

	private string _fragment;

	private string _query;

	private string _path;

	private string _scheme;

	private string _port;

	private string _host;

	private bool _relative;

	public string Origin
	{
		get
		{
			if (_scheme.Is(ProtocolNames.Blob))
			{
				Url url = new Url(Data);
				if (!url.IsInvalid)
				{
					return url.Origin;
				}
			}
			else if (ProtocolNames.IsOriginable(_scheme))
			{
				StringBuilder stringBuilder = Pool.NewStringBuilder();
				if (string.IsNullOrEmpty(_host))
				{
					return stringBuilder.ToPool();
				}
				if (!string.IsNullOrEmpty(_scheme))
				{
					stringBuilder.Append(_scheme).Append(':');
				}
				stringBuilder.Append('/').Append('/').Append(_host);
				if (!string.IsNullOrEmpty(_port))
				{
					stringBuilder.Append(':').Append(_port);
				}
				return stringBuilder.ToPool();
			}
			return null;
		}
	}

	public bool IsInvalid { get; private set; }

	public bool IsRelative
	{
		get
		{
			if (_relative)
			{
				return string.IsNullOrEmpty(_scheme);
			}
			return false;
		}
	}

	public string UserName { get; set; }

	public string Password { get; set; }

	public string Data { get; private set; }

	public string Fragment
	{
		get
		{
			return _fragment;
		}
		set
		{
			if (value == null)
			{
				_fragment = null;
			}
			else
			{
				ParseFragment(value, 0);
			}
		}
	}

	public string Host
	{
		get
		{
			return HostName + (string.IsNullOrEmpty(_port) ? string.Empty : (":" + _port));
		}
		set
		{
			ParseHostName(value ?? string.Empty, 0, onlyHost: false, onlyPort: true);
		}
	}

	public string HostName
	{
		get
		{
			return _host;
		}
		set
		{
			ParseHostName(value ?? string.Empty, 0, onlyHost: true);
		}
	}

	public string Href
	{
		get
		{
			return Serialize();
		}
		set
		{
			IsInvalid = ParseUrl(value ?? string.Empty);
		}
	}

	public string Path
	{
		get
		{
			return _path;
		}
		set
		{
			ParsePath(value ?? string.Empty, 0, onlyPath: true);
		}
	}

	public string Port
	{
		get
		{
			return _port;
		}
		set
		{
			ParsePort(value ?? string.Empty, 0, onlyPort: true);
		}
	}

	public string Scheme
	{
		get
		{
			return _scheme;
		}
		set
		{
			ParseScheme(value ?? string.Empty, onlyScheme: true);
		}
	}

	public string Query
	{
		get
		{
			return _query;
		}
		set
		{
			ParseQuery(value ?? string.Empty, 0, onlyQuery: true);
		}
	}

	public static implicit operator Uri(Url value)
	{
		return new Uri(value.Serialize(), (!value.IsRelative) ? UriKind.Absolute : UriKind.Relative);
	}

	private Url(string scheme, string host, string port)
	{
		Data = string.Empty;
		_path = string.Empty;
		_scheme = scheme;
		_host = host;
		_port = port;
		_relative = ProtocolNames.IsRelative(_scheme);
	}

	public Url(string address)
	{
		IsInvalid = ParseUrl(address);
	}

	public Url(Url baseAddress, string relativeAddress)
	{
		IsInvalid = ParseUrl(relativeAddress, baseAddress);
	}

	public Url(Url address)
	{
		_fragment = address._fragment;
		_query = address._query;
		_path = address._path;
		_scheme = address._scheme;
		_port = address._port;
		_host = address._host;
		UserName = address.UserName;
		Password = address.Password;
		_relative = address._relative;
		Data = address.Data;
	}

	public static Url Create(string address)
	{
		return new Url(address);
	}

	public static Url Convert(Uri uri)
	{
		return new Url(uri.OriginalString);
	}

	public override int GetHashCode()
	{
		return new { _fragment, _query, _path, _scheme, _port, _host, UserName, Password, _relative, Data }.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is Url other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(Url other)
	{
		if (_fragment.Is(other._fragment) && _query.Is(other._query) && _path.Is(other._path) && _scheme.Isi(other._scheme) && _port.Is(other._port) && _host.Isi(other._host) && UserName.Is(other.UserName) && Password.Is(other.Password))
		{
			return Data.Is(other.Data);
		}
		return false;
	}

	public override string ToString()
	{
		return Serialize();
	}

	private string Serialize()
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		if (!string.IsNullOrEmpty(_scheme))
		{
			stringBuilder.Append(_scheme).Append(':');
		}
		if (_relative)
		{
			if (!string.IsNullOrEmpty(_host) || !string.IsNullOrEmpty(_scheme))
			{
				stringBuilder.Append('/').Append('/');
				if (!string.IsNullOrEmpty(UserName) || Password != null)
				{
					stringBuilder.Append(UserName);
					if (Password != null)
					{
						stringBuilder.Append(':').Append(Password);
					}
					stringBuilder.Append('@');
				}
				stringBuilder.Append(_host);
				if (!string.IsNullOrEmpty(_port))
				{
					stringBuilder.Append(':').Append(_port);
				}
				stringBuilder.Append('/');
			}
			stringBuilder.Append(_path);
		}
		else
		{
			stringBuilder.Append(Data);
		}
		if (_query != null)
		{
			stringBuilder.Append('?').Append(_query);
		}
		if (_fragment != null)
		{
			stringBuilder.Append('#').Append(_fragment);
		}
		return stringBuilder.ToPool();
	}

	private bool ParseUrl(string input, Url baseUrl = null)
	{
		Reset(baseUrl ?? DefaultBase);
		return !ParseScheme(input.Trim());
	}

	private void Reset(Url baseUrl)
	{
		Data = string.Empty;
		_scheme = baseUrl._scheme;
		_host = baseUrl._host;
		_path = baseUrl._path;
		_port = baseUrl._port;
		_relative = ProtocolNames.IsRelative(_scheme);
	}

	private bool ParseScheme(string input, bool onlyScheme = false)
	{
		if (input.Length <= 0 || !input[0].IsLetter())
		{
			if (!onlyScheme)
			{
				return RelativeState(input, 0);
			}
			return false;
		}
		for (int i = 1; i < input.Length; i++)
		{
			char c = input[i];
			if (c.IsAlphanumericAscii())
			{
				continue;
			}
			switch (c)
			{
			case '+':
			case '-':
			case '.':
				continue;
			case ':':
			{
				string scheme = _scheme;
				_scheme = input.Substring(0, i).ToLowerInvariant();
				if (!onlyScheme)
				{
					_relative = ProtocolNames.IsRelative(_scheme);
					if (_scheme.Is(ProtocolNames.File))
					{
						_host = string.Empty;
						_port = string.Empty;
						return RelativeState(input, i + 1);
					}
					if (!_relative)
					{
						_host = string.Empty;
						_port = string.Empty;
						_path = string.Empty;
						return ParseSchemeData(input, i + 1);
					}
					if (_scheme.Is(scheme))
					{
						c = input[++i];
						if (c == '/' && i + 2 < input.Length && input[i + 1] == '/')
						{
							return IgnoreSlashesState(input, i + 2);
						}
						return RelativeState(input, i);
					}
					if (i < input.Length - 1 && input[++i] == '/' && ++i < input.Length && input[i] == '/')
					{
						i++;
					}
					return IgnoreSlashesState(input, i);
				}
				return true;
			}
			}
			break;
		}
		if (!onlyScheme)
		{
			return RelativeState(input, 0);
		}
		return false;
	}

	private bool ParseSchemeData(string input, int index)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		for (; index < input.Length; index++)
		{
			char c = input[index];
			switch (c)
			{
			case '?':
				Data = stringBuilder.ToPool();
				return ParseQuery(input, index + 1);
			case '#':
				Data = stringBuilder.ToPool();
				return ParseFragment(input, index + 1);
			case '%':
				if (index + 2 < input.Length && input[index + 1].IsHex() && input[index + 2].IsHex())
				{
					stringBuilder.Append(input[index++]);
					stringBuilder.Append(input[index++]);
					stringBuilder.Append(input[index]);
					continue;
				}
				break;
			}
			if (c.IsInRange(32, 126))
			{
				stringBuilder.Append(c);
			}
			else if (c != '\t' && c != '\n' && c != '\r')
			{
				index += Utf8PercentEncode(stringBuilder, input, index);
			}
		}
		Data = stringBuilder.ToPool();
		return true;
	}

	private bool RelativeState(string input, int index)
	{
		_relative = true;
		if (index != input.Length)
		{
			switch (input[index])
			{
			case '?':
				return ParseQuery(input, index + 1);
			case '#':
				return ParseFragment(input, index + 1);
			case '/':
			case '\\':
				if (index == input.Length - 1)
				{
					return ParsePath(input, index);
				}
				if (input[++index].IsOneOf('/', '\\'))
				{
					if (!_scheme.Is(ProtocolNames.File))
					{
						return IgnoreSlashesState(input, index + 1);
					}
					return ParseFileHost(input, index + 1);
				}
				if (!_scheme.Is(ProtocolNames.File))
				{
					return ParsePath(input, index - 1);
				}
				_host = string.Empty;
				_port = string.Empty;
				return ParsePath(input, index - 1);
			default:
				if (input[index].IsLetter() && _scheme.Is(ProtocolNames.File) && index + 1 < input.Length && input[index + 1].IsOneOf(':', '/') && (index + 2 == input.Length || input[index + 2].IsOneOf('/', '\\', '#', '?')))
				{
					_host = string.Empty;
					_path = string.Empty;
					_port = string.Empty;
				}
				return ParsePath(input, index);
			}
		}
		return true;
	}

	private bool IgnoreSlashesState(string input, int index)
	{
		while (index < input.Length)
		{
			if (!input[index].IsOneOf('\\', '/'))
			{
				return ParseAuthority(input, index);
			}
			index++;
		}
		return false;
	}

	private bool ParseAuthority(string input, int index)
	{
		int index2 = index;
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		string text = null;
		string password = null;
		for (; index < input.Length; index++)
		{
			char c = input[index];
			switch (c)
			{
			case '@':
				if (text == null)
				{
					text = stringBuilder.ToString();
				}
				else
				{
					password = stringBuilder.ToString();
				}
				UserName = text;
				Password = password;
				stringBuilder.Append("%40");
				index2 = index + 1;
				continue;
			case ':':
				if (text == null)
				{
					text = stringBuilder.ToString();
					password = string.Empty;
					stringBuilder.Clear();
					continue;
				}
				goto default;
			default:
				if (c == '%' && index + 2 < input.Length && input[index + 1].IsHex() && input[index + 2].IsHex())
				{
					stringBuilder.Append(input[index++]).Append(input[index++]).Append(input[index]);
					continue;
				}
				if (c.IsOneOf('\t', '\n', '\r'))
				{
					continue;
				}
				if (!c.IsOneOf('/', '\\', '#', '?'))
				{
					if (c != ':' && (c == '#' || c == '?' || c.IsNormalPathCharacter()))
					{
						stringBuilder.Append(c);
					}
					else
					{
						index += Utf8PercentEncode(stringBuilder, input, index);
					}
					continue;
				}
				break;
			}
			break;
		}
		stringBuilder.ToPool();
		return ParseHostName(input, index2);
	}

	private bool ParseFileHost(string input, int index)
	{
		int num = index;
		_path = string.Empty;
		while (index < input.Length)
		{
			char c = input[index];
			if (c == '/' || c == '\\' || c == '#' || c == '?')
			{
				break;
			}
			index++;
		}
		int num2 = index - num;
		if (num2 == 2 && input[index - 2].IsLetter() && (input[index - 1] == '|' || input[index - 1] == ':'))
		{
			return ParsePath(input, index - 2);
		}
		if (num2 != 0)
		{
			_host = SanatizeHost(input, num, num2);
		}
		return ParsePath(input, index);
	}

	private bool ParseHostName(string input, int index, bool onlyHost = false, bool onlyPort = false)
	{
		bool flag = false;
		int num = index;
		while (index < input.Length)
		{
			switch (input[index])
			{
			case ']':
				flag = false;
				break;
			case '[':
				flag = true;
				break;
			case ':':
				if (!flag)
				{
					_host = SanatizeHost(input, num, index - num);
					if (!onlyHost)
					{
						return ParsePort(input, index + 1, onlyPort);
					}
					return true;
				}
				break;
			case '#':
			case '/':
			case '?':
			case '\\':
			{
				_host = SanatizeHost(input, num, index - num);
				bool flag2 = string.IsNullOrEmpty(_host);
				if (!onlyHost)
				{
					if (ParsePath(input, index))
					{
						return !flag2;
					}
					return false;
				}
				return !flag2;
			}
			}
			index++;
		}
		_host = SanatizeHost(input, num, index - num);
		if (!onlyHost)
		{
			_path = string.Empty;
			_query = null;
			_fragment = null;
		}
		return true;
	}

	private bool ParsePort(string input, int index, bool onlyPort = false)
	{
		int num = index;
		while (index < input.Length)
		{
			char c = input[index];
			if (c == '?' || c == '/' || c == '\\' || c == '#')
			{
				break;
			}
			if (c.IsDigit() || c == '\t' || c == '\n' || c == '\r')
			{
				index++;
				continue;
			}
			return false;
		}
		_port = SanatizePort(input, num, index - num);
		if (PortNumbers.GetDefaultPort(_scheme) == _port)
		{
			_port = string.Empty;
		}
		if (!onlyPort)
		{
			_path = string.Empty;
			return ParsePath(input, index);
		}
		return true;
	}

	private bool ParsePath(string input, int index, bool onlyPath = false)
	{
		int num = index;
		if (index < input.Length && (input[index] == '/' || input[index] == '\\'))
		{
			index++;
		}
		List<string> list = new List<string>();
		if (!onlyPath && !string.IsNullOrEmpty(_path) && index - num == 0)
		{
			string[] array = _path.Split('/');
			if (array.Length > 1)
			{
				list.AddRange(array);
				list.RemoveAt(array.Length - 1);
			}
		}
		int count = list.Count;
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		while (index <= input.Length)
		{
			char c = ((index == input.Length) ? '\uffff' : input[index]);
			bool flag = !onlyPath && (c == '#' || c == '?');
			if ((c == '\uffff' || c == '/' || c == '\\') | flag)
			{
				string text = stringBuilder.ToString();
				bool flag2 = false;
				stringBuilder.Clear();
				if (text.Isi("%2e"))
				{
					text = ".";
				}
				else if (text.Isi(UpperDirectoryAlternatives[0]) || text.Isi(UpperDirectoryAlternatives[1]) || text.Isi(UpperDirectoryAlternatives[2]))
				{
					text = "..";
				}
				if (text.Is(".."))
				{
					if (list.Count > 0)
					{
						list.RemoveAt(list.Count - 1);
					}
					flag2 = true;
				}
				else if (!text.Is("."))
				{
					if (_scheme.Is(ProtocolNames.File) && list.Count == count && text.Length == 2 && text[0].IsLetter() && text[1] == '|')
					{
						text = text.Replace('|', ':');
						list.Clear();
					}
					list.Add(text);
				}
				else
				{
					flag2 = true;
				}
				if (flag2 && c != '/' && c != '\\')
				{
					list.Add(string.Empty);
				}
				if (flag)
				{
					break;
				}
			}
			else if (c == '%' && index + 2 < input.Length && input[index + 1].IsHex() && input[index + 2].IsHex())
			{
				stringBuilder.Append(input[index++]);
				stringBuilder.Append(input[index++]);
				stringBuilder.Append(input[index]);
			}
			else if (c != '\t' && c != '\n' && c != '\r')
			{
				if (c.IsNormalPathCharacter())
				{
					stringBuilder.Append(c);
				}
				else
				{
					index += Utf8PercentEncode(stringBuilder, input, index);
				}
			}
			index++;
		}
		stringBuilder.ToPool();
		_path = string.Join("/", list);
		if (index < input.Length)
		{
			if (input[index] == '?')
			{
				return ParseQuery(input, index + 1);
			}
			return ParseFragment(input, index + 1);
		}
		return true;
	}

	private bool ParseQuery(string input, int index, bool onlyQuery = false)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		bool flag = false;
		while (index < input.Length)
		{
			char c = input[index];
			flag = !onlyQuery && input[index] == '#';
			if (flag)
			{
				break;
			}
			if (c.IsNormalQueryCharacter())
			{
				stringBuilder.Append(c);
			}
			else
			{
				index += Utf8PercentEncode(stringBuilder, input, index);
			}
			index++;
		}
		_query = stringBuilder.ToPool();
		if (flag)
		{
			return ParseFragment(input, index + 1);
		}
		return true;
	}

	private bool ParseFragment(string input, int index)
	{
		StringBuilder stringBuilder = Pool.NewStringBuilder();
		while (index < input.Length)
		{
			char c = input[index];
			switch (c)
			{
			default:
				stringBuilder.Append(c);
				break;
			case '\0':
			case '\t':
			case '\n':
			case '\r':
			case '\uffff':
				break;
			}
			index++;
		}
		_fragment = stringBuilder.ToPool();
		return true;
	}

	private static int Utf8PercentEncode(StringBuilder buffer, string source, int index)
	{
		int num = ((!char.IsSurrogatePair(source, index)) ? 1 : 2);
		byte[] bytes = TextEncoding.Utf8.GetBytes(source.Substring(index, num));
		foreach (byte b in bytes)
		{
			buffer.Append('%').Append(b.ToString("X2"));
		}
		return num - 1;
	}

	private static string SanatizeHost(string hostName, int start, int length)
	{
		if (length > 1 && hostName[start] == '[' && hostName[start + length - 1] == ']')
		{
			return hostName.Substring(start, length);
		}
		byte[] array = new byte[4 * length];
		int count = 0;
		int num = start + length;
		for (int i = start; i < num; i++)
		{
			switch (hostName[i])
			{
			case '.':
				array[count++] = (byte)hostName[i];
				continue;
			case '%':
				if (i + 2 < num && hostName[i + 1].IsHex() && hostName[i + 2].IsHex())
				{
					int num2 = hostName[i + 1].FromHex() * 16 + hostName[i + 2].FromHex();
					array[count++] = (byte)num2;
					i += 2;
				}
				else
				{
					array[count++] = 37;
				}
				continue;
			case '\0':
			case '\t':
			case '\n':
			case '\r':
			case ' ':
			case '#':
			case '/':
			case ':':
			case '?':
			case '@':
			case '[':
			case '\\':
			case ']':
				continue;
			}
			if (Symbols.Punycode.TryGetValue(hostName[i], out var value))
			{
				array[count++] = (byte)value;
			}
			else if (!hostName[i].IsAlphanumericAscii())
			{
				int num3 = ((i + 1 >= num || !char.IsSurrogatePair(hostName, i)) ? 1 : 2);
				if (num3 != 1 || hostName[i] == '-' || char.IsLetterOrDigit(hostName[i]))
				{
					byte[] bytes = TextEncoding.Utf8.GetBytes(hostName.Substring(i, num3));
					foreach (byte b in bytes)
					{
						array[count++] = b;
					}
					i += num3 - 1;
				}
			}
			else
			{
				array[count++] = (byte)char.ToLowerInvariant(hostName[i]);
			}
		}
		return TextEncoding.Utf8.GetString(array, 0, count);
	}

	private static string SanatizePort(string port, int start, int length)
	{
		char[] array = new char[length];
		int num = 0;
		int num2 = start + length;
		for (int i = start; i < num2; i++)
		{
			switch (port[i])
			{
			case '\t':
			case '\n':
			case '\r':
				continue;
			}
			if (num == 1 && array[0] == '0')
			{
				array[0] = port[i];
			}
			else
			{
				array[num++] = port[i];
			}
		}
		return new string(array, 0, num);
	}
}
