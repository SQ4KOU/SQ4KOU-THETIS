using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Thetis;

public static class MultiMeterIO
{
	[Serializable]
	public enum MMIODirection
	{
		IN,
		OUT,
		BOTH
	}

	[Serializable]
	public enum MMIOFormat
	{
		JSON,
		XML,
		RAW,
		LAST
	}

	[Serializable]
	public enum MMIOType
	{
		UDP_LISTENER,
		TCPIP_LISTENER,
		SERIAL,
		TCPIP_CLIENT,
		REQUESTER
	}

	[Serializable]
	public enum MMIOTerminator
	{
		NONE,
		CR,
		LF,
		CRLF,
		CUSTOM,
		LAST
	}

	[Serializable]
	public class clsMMIO
	{
		private Guid _guid;

		private MMIODirection _direction;

		private string _ip;

		private int _port;

		private string _udp_endpoint_ip;

		private int _udp_endpoint_port;

		private MMIOFormat _format_in;

		private MMIOFormat _format_out;

		private MMIOType _type;

		private bool _listener_active;

		private string _four_char;

		private bool _enabled;

		private MMIOTerminator _terminator_in;

		private MMIOTerminator _terminator_out;

		private string _custom_terminator_in;

		private string _custom_terminator_out;

		private string _custom_terminator_parsed_in;

		private string _custom_terminator_parsed_out;

		private IPEndPoint _udp_endpoint;

		private string _com_port;

		private int _baud_rate;

		private int _data_bits;

		private StopBits _stop_bits;

		private Parity _parity;

		private ConcurrentDictionary<string, object> _io_variables;

		[NonSerialized]
		private ConcurrentQueue<string> _outbound_queue;

		public Guid Guid
		{
			get
			{
				return _guid;
			}
			set
			{
				_guid = value;
				_four_char = Common.FourChar(_ip, _port, _guid);
			}
		}

		public MMIODirection Direction
		{
			get
			{
				return _direction;
			}
			set
			{
				_direction = value;
				refreshUdpEndpoint();
			}
		}

		public string IP
		{
			get
			{
				return _ip;
			}
			set
			{
				_ip = value;
				if (string.IsNullOrEmpty(_four_char))
				{
					_four_char = Common.FourChar(_ip, _port, _guid);
				}
			}
		}

		public int Port
		{
			get
			{
				return _port;
			}
			set
			{
				_port = value;
				if (string.IsNullOrEmpty(_four_char))
				{
					_four_char = Common.FourChar(_ip, _port, _guid);
				}
			}
		}

		public string UdpEndpointIP
		{
			get
			{
				return _udp_endpoint_ip;
			}
			set
			{
				_udp_endpoint_ip = value;
				refreshUdpEndpoint();
			}
		}

		public int UdpEndpointPort
		{
			get
			{
				return _udp_endpoint_port;
			}
			set
			{
				_udp_endpoint_port = value;
				refreshUdpEndpoint();
			}
		}

		public string ComPort
		{
			get
			{
				return _com_port;
			}
			set
			{
				_com_port = value;
			}
		}

		public int BaudRate
		{
			get
			{
				return _baud_rate;
			}
			set
			{
				_baud_rate = value;
			}
		}

		public int DataBits
		{
			get
			{
				return _data_bits;
			}
			set
			{
				_data_bits = value;
			}
		}

		public StopBits StopBits
		{
			get
			{
				return _stop_bits;
			}
			set
			{
				_stop_bits = value;
			}
		}

		public Parity Parity
		{
			get
			{
				return _parity;
			}
			set
			{
				_parity = value;
			}
		}

		public IPEndPoint UDPEndPoint => _udp_endpoint;

		public MMIOFormat FormatIn
		{
			get
			{
				return _format_in;
			}
			set
			{
				_format_in = value;
			}
		}

		public MMIOFormat FormatOut
		{
			get
			{
				return _format_out;
			}
			set
			{
				_format_out = value;
			}
		}

		public MMIOTerminator TerminatorIn
		{
			get
			{
				return _terminator_in;
			}
			set
			{
				_terminator_in = value;
			}
		}

		public MMIOTerminator TerminatorOut
		{
			get
			{
				return _terminator_out;
			}
			set
			{
				_terminator_out = value;
			}
		}

		public MMIOType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public bool Active
		{
			get
			{
				return _listener_active;
			}
			set
			{
				_listener_active = value;
			}
		}

		public string FourChar => _four_char;

		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public string CustomTerminatorParsedIn
		{
			get
			{
				if (!string.IsNullOrEmpty(_custom_terminator_parsed_in))
				{
					return _custom_terminator_parsed_in;
				}
				return "\0";
			}
		}

		public string CustomTerminatorIn
		{
			get
			{
				return _custom_terminator_in;
			}
			set
			{
				_custom_terminator_in = value;
				_custom_terminator_parsed_in = _custom_terminator_in.Replace("\\n", "\n");
				_custom_terminator_parsed_in = _custom_terminator_parsed_in.Replace("\\r", "\r");
				_custom_terminator_parsed_in = _custom_terminator_parsed_in.Replace("\\0", "\0");
			}
		}

		public bool OutboundQueueEmpty => _outbound_queue.Count == 0;

		public string CustomTerminatorParsedOut
		{
			get
			{
				if (!string.IsNullOrEmpty(_custom_terminator_parsed_out))
				{
					return _custom_terminator_parsed_out;
				}
				return "\0";
			}
		}

		public string CustomTerminatorOut
		{
			get
			{
				return _custom_terminator_out;
			}
			set
			{
				_custom_terminator_out = value;
				_custom_terminator_parsed_out = _custom_terminator_out.Replace("\\n", "\n");
				_custom_terminator_parsed_out = _custom_terminator_parsed_out.Replace("\\r", "\r");
				_custom_terminator_parsed_out = _custom_terminator_parsed_out.Replace("\\0", "\0");
			}
		}

		public clsMMIO()
		{
			init();
			_four_char = Common.FourChar(_ip, _port, _guid);
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			_outbound_queue = new ConcurrentQueue<string>();
		}

		private void init()
		{
			_enabled = true;
			_guid = Guid.NewGuid();
			_type = MMIOType.UDP_LISTENER;
			_ip = "127.0.0.1";
			_port = 9000;
			_udp_endpoint_ip = "127.0.0.1";
			_udp_endpoint_port = 10000;
			_listener_active = false;
			_format_in = MMIOFormat.JSON;
			_format_out = MMIOFormat.JSON;
			_direction = MMIODirection.IN;
			_terminator_in = MMIOTerminator.NONE;
			_terminator_out = MMIOTerminator.NONE;
			_custom_terminator_in = "";
			_custom_terminator_out = "";
			_custom_terminator_parsed_in = "";
			_custom_terminator_parsed_out = "";
			_udp_endpoint = null;
			_com_port = "";
			_baud_rate = 9600;
			_data_bits = 8;
			_stop_bits = StopBits.One;
			_parity = Parity.None;
			_io_variables = new ConcurrentDictionary<string, object>();
			_outbound_queue = new ConcurrentQueue<string>();
		}

		public clsMMIO(MMIOType type, string ip, int port, bool enabled)
		{
			init();
			_enabled = enabled;
			_type = type;
			_ip = ip;
			_port = port;
			_four_char = Common.FourChar(_ip, _port, _guid);
		}

		public clsMMIO(MMIOType type, string com_port, int baud_rate, int data_bits, StopBits stop_bits, Parity parity, bool enabled)
		{
			init();
			_enabled = enabled;
			_type = type;
			_ip = "";
			_port = 0;
			_com_port = com_port;
			_baud_rate = baud_rate;
			_data_bits = data_bits;
			_stop_bits = stop_bits;
			_parity = parity;
			_four_char = Common.FourChar(com_port, baud_rate + data_bits, _guid);
		}

		private void refreshUdpEndpoint()
		{
			if (_direction == MMIODirection.OUT || _direction == MMIODirection.BOTH)
			{
				_udp_endpoint = new IPEndPoint(IPAddress.Parse(_udp_endpoint_ip), _udp_endpoint_port);
			}
			else
			{
				_udp_endpoint = null;
			}
		}

		public void EnqueueOutbound(string data)
		{
			if (_enabled && _direction != MMIODirection.IN)
			{
				if (_outbound_queue.Count >= 1000)
				{
					_outbound_queue.TryDequeue(out var _);
				}
				_outbound_queue.Enqueue(data);
			}
		}

		public string DequeueOutbound()
		{
			if (!_enabled || _direction == MMIODirection.IN)
			{
				return "";
			}
			if (_outbound_queue.TryDequeue(out var result))
			{
				return result;
			}
			return "";
		}

		public bool StartConnection()
		{
			bool result = false;
			switch (_type)
			{
			case MMIOType.UDP_LISTENER:
				refreshUdpEndpoint();
				result = StartListeningUDP(this);
				break;
			case MMIOType.TCPIP_LISTENER:
				result = StartListeningTCPIP(this);
				break;
			case MMIOType.TCPIP_CLIENT:
				result = StartTcpClient(this);
				break;
			case MMIOType.SERIAL:
				result = StartSerialPort(this);
				break;
			}
			return result;
		}

		public void StopConnection()
		{
			MultiMeterIO.StopConnection(_guid);
		}

		public bool SetVariable(string key, object value)
		{
			if (_io_variables.ContainsKey(key))
			{
				_io_variables[key] = value;
				return true;
			}
			return _io_variables.TryAdd(key, value);
		}

		public object GetVariable(string key, string precision_format = "")
		{
			if (_io_variables.ContainsKey(key))
			{
				if (precision_format == "")
				{
					precision_format = "0.0#####";
				}
				object obj = _io_variables[key];
				string value = ((obj is int) ? obj.ToString() : ((!(obj is float num)) ? ((!(obj is double num2)) ? ((!(obj is bool flag)) ? obj.ToString() : flag.ToString().ToLower()) : num2.ToString(precision_format)) : num.ToString(precision_format)));
				Type type = DetermineType(value);
				return ConvertToType(value, type);
			}
			return false;
		}

		public Type DetermineType(string value)
		{
			CultureInfo provider = new CultureInfo("fr-FR");
			NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands;
			if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) || int.TryParse(value, NumberStyles.Integer, provider, out result))
			{
				return typeof(int);
			}
			if (float.TryParse(value, style, CultureInfo.InvariantCulture, out var result2) || float.TryParse(value, style, provider, out result2))
			{
				if (value.Contains(".") || value.Contains(","))
				{
					char c = (value.Contains(".") ? '.' : ',');
					string[] array = value.Split(c);
					if (array.Length == 2 && array[1].Length > 7)
					{
						return typeof(double);
					}
				}
				return typeof(float);
			}
			if (double.TryParse(value, style, CultureInfo.InvariantCulture, out var result3) || double.TryParse(value, style, provider, out result3))
			{
				return typeof(double);
			}
			if (bool.TryParse(value, out var _))
			{
				return typeof(bool);
			}
			return typeof(string);
		}

		public object ConvertToType(string value, Type type)
		{
			CultureInfo culture = new CultureInfo("fr-FR");
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			try
			{
				TypeConverter converter = TypeDescriptor.GetConverter(type);
				try
				{
					return converter.ConvertFromString(null, invariantCulture, value);
				}
				catch
				{
					try
					{
						return converter.ConvertFromString(null, CultureInfo.CurrentCulture, value);
					}
					catch
					{
						return converter.ConvertFromString(null, culture, value);
					}
				}
			}
			catch (Exception)
			{
				if (type.IsEnum)
				{
					return Enum.Parse(type, value);
				}
				throw new ArgumentException("Cannot convert the string to the specified type.", "value");
			}
		}

		public ConcurrentDictionary<string, object> Variables()
		{
			return _io_variables;
		}

		public string VariableValueType(object obj, string float_precision = "")
		{
			if (float_precision == "")
			{
				float_precision = "0.0#####";
			}
			if (obj is int num)
			{
				return num.ToString();
			}
			if (obj is float num2)
			{
				return num2.ToString(float_precision);
			}
			if (obj is double num3)
			{
				return num3.ToString(float_precision);
			}
			if (obj is bool flag)
			{
				return flag.ToString().ToLower();
			}
			return obj.ToString();
		}

		public void RemoveVariable(string key)
		{
			_io_variables.TryRemove(key, out var _);
		}
	}

	private class TcpListener
	{
		private Guid _guid;

		private MMIOType _type;

		private string _ip;

		private int _port;

		private System.Net.Sockets.TcpListener _tcpListener;

		private TcpClient _tcpClient;

		private Thread _listenerThread;

		private volatile bool _isRunning;

		private bool clientConnected
		{
			get
			{
				try
				{
					return _tcpClient.Connected;
				}
				catch
				{
					return false;
				}
			}
		}

		public event Action<Guid, string> ReceivedDataString;

		public event Action<Guid> TransmittedData;

		public event Action<Guid, MMIOType, bool> ConnectorRunning;

		public TcpListener(Guid guid, MMIOType type, string ip, int port)
		{
			_guid = guid;
			_type = type;
			_ip = ip;
			_port = port;
			_tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Parse(ip), port);
		}

		public void Start()
		{
			_isRunning = true;
			_listenerThread = new Thread(listen);
			_listenerThread.IsBackground = true;
			_listenerThread.Start();
			ConnectorRunning?.Invoke(_guid, _type, arg3: true);
		}

		public void Stop()
		{
			_isRunning = false;
			if (_tcpClient != null)
			{
				_tcpClient.Close();
			}
			_tcpListener.Stop();
			_listenerThread.Join();
			ConnectorRunning?.Invoke(_guid, _type, arg3: false);
		}

		private void listen()
		{
			try
			{
				bool flag = true;
				DateTime utcNow = DateTime.UtcNow;
				_tcpListener.Start();
				while (_isRunning)
				{
					if (!_tcpListener.Pending())
					{
						Thread.Sleep(50);
						continue;
					}
					try
					{
						_tcpClient = _tcpListener.AcceptTcpClient();
						ClientConnected?.Invoke(_guid);
						_tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, optionValue: true);
						if (flag)
						{
							_tcpListener.Stop();
							flag = false;
						}
						NetworkStream stream = _tcpClient.GetStream();
						string text = "";
						while (clientConnected && _isRunning)
						{
							bool flag2 = true;
							bool num = _mmio_data[_guid].Direction == MMIODirection.IN || _mmio_data[_guid].Direction == MMIODirection.BOTH;
							bool flag3 = _mmio_data[_guid].Direction == MMIODirection.OUT || _mmio_data[_guid].Direction == MMIODirection.BOTH;
							if (num)
							{
								try
								{
									if (stream.DataAvailable)
									{
										byte[] array = new byte[1024];
										int num2 = stream.Read(array, 0, array.Length);
										if (num2 > 0)
										{
											utcNow = DateTime.UtcNow;
											string text2 = Encoding.UTF8.GetString(array, 0, num2);
											flag2 = false;
											string text3 = "";
											switch (_mmio_data[_guid].TerminatorIn)
											{
											case MMIOTerminator.NONE:
												text3 = "";
												break;
											case MMIOTerminator.CR:
												text3 = "\r";
												break;
											case MMIOTerminator.LF:
												text3 = "\n";
												break;
											case MMIOTerminator.CRLF:
												text3 = "\r\n";
												break;
											case MMIOTerminator.CUSTOM:
												text3 = _mmio_data[_guid].CustomTerminatorParsedIn;
												break;
											}
											text += text2;
											if (string.IsNullOrEmpty(text3))
											{
												ReceivedDataString?.Invoke(_guid, text);
												text = "";
											}
											else
											{
												for (int num3 = text.IndexOf(text3); num3 > -1; num3 = text.IndexOf(text3))
												{
													ReceivedDataString?.Invoke(_guid, text.Substring(0, num3));
													text = text.Substring(num3 + text3.Length);
												}
											}
											if (text.Length >= 32768)
											{
												text = text.Substring(16384);
											}
										}
									}
								}
								catch (Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
								{
									break;
								}
							}
							if (flag3 && !_mmio_data[_guid].OutboundQueueEmpty)
							{
								string text4 = "";
								int num4 = 0;
								string text5 = "";
								switch (_mmio_data[_guid].TerminatorOut)
								{
								case MMIOTerminator.NONE:
									text5 = "";
									break;
								case MMIOTerminator.CR:
									text5 = "\r";
									break;
								case MMIOTerminator.LF:
									text5 = "\n";
									break;
								case MMIOTerminator.CRLF:
									text5 = "\r\n";
									break;
								case MMIOTerminator.CUSTOM:
									text5 = _mmio_data[_guid].CustomTerminatorParsedOut;
									break;
								}
								while (!_mmio_data[_guid].OutboundQueueEmpty)
								{
									text4 = text4 + _mmio_data[_guid].DequeueOutbound() + text5;
									num4++;
									if (num4 == 20)
									{
										break;
									}
								}
								if (text4.Length > 0)
								{
									utcNow = DateTime.UtcNow;
									flag2 = false;
									byte[] bytes = Encoding.ASCII.GetBytes(text4);
									try
									{
										stream.Write(bytes, 0, bytes.Length);
										TransmittedData?.Invoke(_guid);
									}
									catch (Exception ex2) when (ex2 is SocketException || ex2 is IOException || ex2 is ObjectDisposedException)
									{
										break;
									}
								}
							}
							if ((DateTime.UtcNow - utcNow).TotalMilliseconds > 5000.0 && clientConnected)
							{
								byte[] bytes2 = Encoding.ASCII.GetBytes("\0");
								try
								{
									stream.Write(bytes2, 0, bytes2.Length);
									utcNow = DateTime.UtcNow;
								}
								catch (Exception ex3) when (ex3 is SocketException || ex3 is IOException || ex3 is ObjectDisposedException)
								{
									break;
								}
							}
							if (flag2)
							{
								Thread.Sleep(50);
							}
							else
							{
								Thread.Sleep(1);
							}
						}
						ClientDisconnected?.Invoke(_guid);
						if (_isRunning && !flag)
						{
							_tcpListener.Start();
						}
					}
					catch (Exception ex4) when (ex4 is SocketException || ex4 is IOException || ex4 is ObjectDisposedException)
					{
						if (_tcpClient != null)
						{
							_tcpClient.Close();
						}
					}
				}
			}
			catch (Exception ex5) when (ex5 is SocketException || ex5 is ObjectDisposedException)
			{
			}
			catch (Exception)
			{
			}
			finally
			{
				if (_tcpClient != null)
				{
					_tcpClient.Close();
				}
				_tcpListener.Stop();
			}
		}
	}

	public class TcpClientHandler
	{
		private Guid _guid;

		private MMIOType _type;

		private string _ip;

		private int _port;

		private TcpClient _tcpClient;

		private Thread _clientThread;

		private volatile bool _isRunning;

		private NetworkStream _networkStream;

		private bool clientConnected
		{
			get
			{
				bool result = false;
				try
				{
					result = _tcpClient.Connected;
				}
				catch
				{
				}
				return result;
			}
		}

		public event Action<Guid, string> ReceivedDataString;

		public event Action<Guid> TransmittedData;

		public event Action<Guid, MMIOType, bool> ConnectorRunning;

		public TcpClientHandler(Guid guid, MMIOType type, string ip, int port)
		{
			_guid = guid;
			_type = type;
			_ip = ip;
			_port = port;
		}

		public void Start()
		{
			_isRunning = true;
			_clientThread = new Thread(Connect);
			_clientThread.IsBackground = true;
			_clientThread.Start();
			ConnectorRunning?.Invoke(_guid, _type, arg3: true);
		}

		public void Stop()
		{
			_isRunning = false;
			if (_tcpClient != null)
			{
				_tcpClient.Close();
			}
			_clientThread.Join();
			ConnectorRunning?.Invoke(_guid, _type, arg3: false);
		}

		private void Connect()
		{
			bool flag = true;
			while (_isRunning & flag)
			{
				try
				{
					flag = false;
					_tcpClient = new TcpClient();
					_tcpClient.Connect(_ip, _port);
					_networkStream = _tcpClient.GetStream();
					DateTime utcNow = DateTime.UtcNow;
					string text = "";
					while (_isRunning && clientConnected)
					{
						bool flag2 = true;
						bool num = _mmio_data[_guid].Direction == MMIODirection.IN || _mmio_data[_guid].Direction == MMIODirection.BOTH;
						bool flag3 = _mmio_data[_guid].Direction == MMIODirection.OUT || _mmio_data[_guid].Direction == MMIODirection.BOTH;
						if (num)
						{
							try
							{
								if (_networkStream.DataAvailable)
								{
									byte[] array = new byte[1024];
									int num2 = _networkStream.Read(array, 0, array.Length);
									if (num2 > 0)
									{
										utcNow = DateTime.UtcNow;
										string text2 = Encoding.UTF8.GetString(array, 0, num2);
										flag2 = false;
										string text3 = "";
										switch (_mmio_data[_guid].TerminatorIn)
										{
										case MMIOTerminator.NONE:
											text3 = "";
											break;
										case MMIOTerminator.CR:
											text3 = "\r";
											break;
										case MMIOTerminator.LF:
											text3 = "\n";
											break;
										case MMIOTerminator.CRLF:
											text3 = "\r\n";
											break;
										case MMIOTerminator.CUSTOM:
											text3 = _mmio_data[_guid].CustomTerminatorParsedIn;
											break;
										}
										text += text2;
										if (string.IsNullOrEmpty(text3))
										{
											ReceivedDataString?.Invoke(_guid, text);
											text = "";
										}
										else
										{
											for (int num3 = text.IndexOf(text3); num3 > -1; num3 = text.IndexOf(text3))
											{
												ReceivedDataString?.Invoke(_guid, text.Substring(0, num3));
												text = text.Substring(num3 + text3.Length);
											}
										}
										if (text.Length >= 32768)
										{
											text = text.Substring(16384);
										}
									}
								}
							}
							catch (Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
							{
								if (_tcpClient != null && !clientConnected)
								{
									flag = true;
								}
								break;
							}
						}
						if (flag3 && !_mmio_data[_guid].OutboundQueueEmpty)
						{
							string text4 = "";
							int num4 = 0;
							string text5 = "";
							switch (_mmio_data[_guid].TerminatorOut)
							{
							case MMIOTerminator.NONE:
								text5 = "";
								break;
							case MMIOTerminator.CR:
								text5 = "\r";
								break;
							case MMIOTerminator.LF:
								text5 = "\n";
								break;
							case MMIOTerminator.CRLF:
								text5 = "\r\n";
								break;
							case MMIOTerminator.CUSTOM:
								text5 = _mmio_data[_guid].CustomTerminatorParsedOut;
								break;
							}
							while (!_mmio_data[_guid].OutboundQueueEmpty)
							{
								text4 = text4 + _mmio_data[_guid].DequeueOutbound() + text5;
								num4++;
								if (num4 == 20)
								{
									break;
								}
							}
							if (text4.Length > 0)
							{
								utcNow = DateTime.UtcNow;
								flag2 = false;
								byte[] bytes = Encoding.ASCII.GetBytes(text4);
								try
								{
									_networkStream.Write(bytes, 0, bytes.Length);
									TransmittedData?.Invoke(_guid);
								}
								catch (Exception ex2) when (ex2 is SocketException || ex2 is IOException || ex2 is ObjectDisposedException)
								{
									if (_tcpClient != null && !clientConnected)
									{
										flag = true;
									}
									break;
								}
							}
						}
						if ((DateTime.UtcNow - utcNow).TotalMilliseconds > 5000.0 && clientConnected)
						{
							byte[] bytes2 = Encoding.ASCII.GetBytes("\0");
							try
							{
								_networkStream.Write(bytes2, 0, bytes2.Length);
								utcNow = DateTime.UtcNow;
							}
							catch (Exception ex3) when (ex3 is SocketException || ex3 is IOException || ex3 is ObjectDisposedException)
							{
								if (_tcpClient != null && !clientConnected)
								{
									flag = true;
								}
								break;
							}
						}
						if (flag2)
						{
							Thread.Sleep(50);
						}
						else
						{
							Thread.Sleep(1);
						}
					}
				}
				catch (Exception ex4) when (ex4 is SocketException || ex4 is ObjectDisposedException)
				{
					if (_tcpClient != null && !clientConnected)
					{
						flag = true;
					}
				}
				catch (Exception)
				{
					if (_tcpClient != null && !clientConnected)
					{
						flag = true;
					}
				}
				finally
				{
					if (_tcpClient != null)
					{
						_tcpClient.Close();
					}
				}
				if (flag)
				{
					Thread.Sleep(100);
				}
			}
		}
	}

	private class UdpListener
	{
		private Guid _guid;

		private string _ip;

		private int _port;

		private MMIOType _type;

		private UdpClient _udpClient;

		private Thread _listenerThread;

		private volatile bool _isRunning;

		public event Action<Guid, string> ReceivedDataString;

		public event Action<Guid> TransmittedData;

		public event Action<Guid, MMIOType, bool> ConnectorRunning;

		public UdpListener(Guid guid, MMIOType type, string ip, int port)
		{
			_guid = guid;
			_type = type;
			_ip = ip;
			_port = port;
			_udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port));
		}

		public void Start()
		{
			_isRunning = true;
			_listenerThread = new Thread(listen);
			_listenerThread.IsBackground = true;
			_listenerThread.Start();
			ConnectorRunning?.Invoke(_guid, _type, arg3: true);
		}

		public void Stop()
		{
			_isRunning = false;
			_udpClient.Close();
			_listenerThread.Join();
			ConnectorRunning?.Invoke(_guid, _type, arg3: false);
		}

		private void listen()
		{
			string text = "";
			UdpClient udpClient = null;
			try
			{
				while (_isRunning)
				{
					bool num = _mmio_data[_guid].Direction == MMIODirection.IN || _mmio_data[_guid].Direction == MMIODirection.BOTH;
					bool flag = _mmio_data[_guid].Direction == MMIODirection.OUT || _mmio_data[_guid].Direction == MMIODirection.BOTH;
					bool flag2 = true;
					if (num)
					{
						try
						{
							if (_udpClient.Available > 0)
							{
								IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
								byte[] bytes = _udpClient.Receive(ref remoteEP);
								string text2 = Encoding.UTF8.GetString(bytes);
								flag2 = false;
								string text3 = "";
								switch (_mmio_data[_guid].TerminatorIn)
								{
								case MMIOTerminator.NONE:
									text3 = "";
									break;
								case MMIOTerminator.CR:
									text3 = "\r";
									break;
								case MMIOTerminator.LF:
									text3 = "\n";
									break;
								case MMIOTerminator.CRLF:
									text3 = "\r\n";
									break;
								case MMIOTerminator.CUSTOM:
									text3 = _mmio_data[_guid].CustomTerminatorParsedIn;
									break;
								}
								text += text2;
								if (string.IsNullOrEmpty(text3))
								{
									ReceivedDataString?.Invoke(_guid, text);
									text = "";
								}
								else
								{
									for (int num2 = text.IndexOf(text3); num2 > -1; num2 = text.IndexOf(text3))
									{
										ReceivedDataString?.Invoke(_guid, text.Substring(0, num2));
										text = text.Substring(num2 + text3.Length);
									}
								}
								if (text.Length >= 32768)
								{
									text = text.Substring(16384);
								}
							}
						}
						catch (Exception ex) when (ex is SocketException || ex is IOException || ex is ObjectDisposedException)
						{
							if (!_isRunning)
							{
								break;
							}
						}
					}
					if (flag && _mmio_data[_guid].UDPEndPoint != null)
					{
						if (udpClient == null)
						{
							udpClient = new UdpClient();
							udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Debug, optionValue: true);
						}
						if (!_mmio_data[_guid].OutboundQueueEmpty)
						{
							string text4 = "";
							int num3 = 0;
							string text5 = "";
							switch (_mmio_data[_guid].TerminatorOut)
							{
							case MMIOTerminator.NONE:
								text5 = "";
								break;
							case MMIOTerminator.CR:
								text5 = "\r";
								break;
							case MMIOTerminator.LF:
								text5 = "\n";
								break;
							case MMIOTerminator.CRLF:
								text5 = "\r\n";
								break;
							case MMIOTerminator.CUSTOM:
								text5 = _mmio_data[_guid].CustomTerminatorParsedOut;
								break;
							}
							while (!_mmio_data[_guid].OutboundQueueEmpty)
							{
								text4 = text4 + _mmio_data[_guid].DequeueOutbound() + text5;
								num3++;
								if (num3 == 20)
								{
									break;
								}
							}
							if (text4.Length > 0)
							{
								flag2 = false;
								byte[] bytes2 = Encoding.ASCII.GetBytes(text4);
								try
								{
									udpClient.Send(bytes2, bytes2.Length, _mmio_data[_guid].UDPEndPoint);
									TransmittedData?.Invoke(_guid);
								}
								catch (Exception ex2) when (ex2 is SocketException || ex2 is IOException || ex2 is ObjectDisposedException)
								{
									if (!_isRunning)
									{
										break;
									}
								}
							}
						}
					}
					else if (udpClient != null)
					{
						try
						{
							udpClient.Close();
						}
						catch (Exception ex3) when (ex3 is SocketException || ex3 is IOException || ex3 is ObjectDisposedException)
						{
						}
						udpClient = null;
					}
					if (flag2)
					{
						Thread.Sleep(50);
					}
					else
					{
						Thread.Sleep(1);
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}

	public class SerialPortHandler
	{
		private Guid _guid;

		private MMIOType _type;

		private string _comPort;

		private int _baudRate;

		private int _dataBits;

		private StopBits _stopBits;

		private Parity _parity;

		private SerialPort _serialPort;

		private Thread _serialThread;

		private volatile bool _isRunning;

		public event Action<Guid, string> ReceivedDataString;

		public event Action<Guid> TransmittedData;

		public event Action<Guid, MMIOType, bool> ConnectorRunning;

		public SerialPortHandler(Guid guid, MMIOType type, string comPort, int baudRate, int dataBits, StopBits stopBits, Parity parity)
		{
			_guid = guid;
			_type = type;
			_comPort = comPort;
			_baudRate = baudRate;
			_dataBits = dataBits;
			_stopBits = stopBits;
			_parity = parity;
			_serialPort = new SerialPort(comPort, baudRate, parity, dataBits, stopBits);
			_serialPort.ReadTimeout = 1000;
			_serialPort.WriteTimeout = 1000;
		}

		public void Start()
		{
			_isRunning = true;
			_serialThread = new Thread(Connect);
			_serialThread.IsBackground = true;
			_serialThread.Start();
			ConnectorRunning?.Invoke(_guid, _type, arg3: true);
		}

		public void Stop()
		{
			_isRunning = false;
			if (_serialPort != null && _serialPort.IsOpen)
			{
				_serialPort.Close();
			}
			_serialThread.Join();
			ConnectorRunning?.Invoke(_guid, _type, arg3: false);
		}

		private void Connect()
		{
			try
			{
				_serialPort.Open();
				DateTime utcNow = DateTime.UtcNow;
				string text = "";
				while (_isRunning && _serialPort.IsOpen)
				{
					bool flag = true;
					bool num = _mmio_data[_guid].Direction == MMIODirection.IN || _mmio_data[_guid].Direction == MMIODirection.BOTH;
					bool flag2 = _mmio_data[_guid].Direction == MMIODirection.OUT || _mmio_data[_guid].Direction == MMIODirection.BOTH;
					if (num)
					{
						try
						{
							if (_serialPort.BytesToRead > 0)
							{
								byte[] array = new byte[1024];
								int num2 = _serialPort.Read(array, 0, array.Length);
								if (num2 > 0)
								{
									utcNow = DateTime.UtcNow;
									string text2 = Encoding.UTF8.GetString(array, 0, num2);
									flag = false;
									string text3 = "";
									switch (_mmio_data[_guid].TerminatorIn)
									{
									case MMIOTerminator.NONE:
										text3 = "";
										break;
									case MMIOTerminator.CR:
										text3 = "\r";
										break;
									case MMIOTerminator.LF:
										text3 = "\n";
										break;
									case MMIOTerminator.CRLF:
										text3 = "\r\n";
										break;
									case MMIOTerminator.CUSTOM:
										text3 = _mmio_data[_guid].CustomTerminatorParsedIn;
										break;
									}
									text += text2;
									if (string.IsNullOrEmpty(text3))
									{
										ReceivedDataString?.Invoke(_guid, text);
										text = "";
									}
									else
									{
										for (int num3 = text.IndexOf(text3); num3 > -1; num3 = text.IndexOf(text3))
										{
											ReceivedDataString?.Invoke(_guid, text.Substring(0, num3));
											text = text.Substring(num3 + text3.Length);
										}
									}
									if (text.Length >= 32768)
									{
										text = text.Substring(16384);
									}
								}
							}
						}
						catch (Exception ex) when (ex is IOException || ex is InvalidOperationException)
						{
							break;
						}
					}
					if (flag2 && !_mmio_data[_guid].OutboundQueueEmpty)
					{
						string text4 = "";
						int num4 = 0;
						string text5 = "";
						switch (_mmio_data[_guid].TerminatorOut)
						{
						case MMIOTerminator.NONE:
							text5 = "";
							break;
						case MMIOTerminator.CR:
							text5 = "\r";
							break;
						case MMIOTerminator.LF:
							text5 = "\n";
							break;
						case MMIOTerminator.CRLF:
							text5 = "\r\n";
							break;
						case MMIOTerminator.CUSTOM:
							text5 = _mmio_data[_guid].CustomTerminatorParsedOut;
							break;
						}
						while (!_mmio_data[_guid].OutboundQueueEmpty)
						{
							text4 = text4 + _mmio_data[_guid].DequeueOutbound() + text5;
							num4++;
							if (num4 == 20)
							{
								break;
							}
						}
						if (text4.Length > 0)
						{
							utcNow = DateTime.UtcNow;
							flag = false;
							byte[] bytes = Encoding.ASCII.GetBytes(text4);
							try
							{
								_serialPort.Write(bytes, 0, bytes.Length);
								TransmittedData?.Invoke(_guid);
							}
							catch (Exception ex2) when (ex2 is IOException || ex2 is InvalidOperationException)
							{
								break;
							}
						}
					}
					if ((DateTime.UtcNow - utcNow).TotalMilliseconds > 5000.0 && _serialPort.IsOpen)
					{
						byte[] bytes2 = Encoding.ASCII.GetBytes("\0");
						try
						{
							_serialPort.Write(bytes2, 0, bytes2.Length);
							utcNow = DateTime.UtcNow;
						}
						catch (Exception ex3) when (ex3 is IOException || ex3 is InvalidOperationException)
						{
							break;
						}
					}
					if (flag)
					{
						Thread.Sleep(50);
					}
					else
					{
						Thread.Sleep(1);
					}
				}
			}
			catch (Exception ex4) when (ex4 is IOException || ex4 is InvalidOperationException)
			{
			}
			catch (Exception)
			{
			}
			finally
			{
				if (_serialPort != null && _serialPort.IsOpen)
				{
					_serialPort.Close();
				}
			}
		}

		public static List<string> GetAvailableComPorts()
		{
			return new List<string>(SerialPort.GetPortNames());
		}
	}

	private const int DELAY = 100;

	private static ConcurrentDictionary<Guid, UdpListener> _udp_listeners;

	private static ConcurrentDictionary<Guid, TcpListener> _tcpip_listeners;

	private static ConcurrentDictionary<Guid, TcpClientHandler> _tcpip_clients;

	private static ConcurrentDictionary<Guid, SerialPortHandler> _serial_ports;

	private static ConcurrentDictionary<Guid, clsMMIO> _mmio_data;

	private static readonly object _connectionLock;

	public static ConcurrentDictionary<Guid, clsMMIO> Data => _mmio_data;

	public static event Action<Guid> ClientConnected;

	public static event Action<Guid> ClientDisconnected;

	public static event Action<Guid, string> ReceivedDataString;

	public static event Action<Guid> TransmittedData;

	public static event Action<Guid, MMIOType, bool> ConnectorRunning;

	static MultiMeterIO()
	{
		_connectionLock = new object();
		_udp_listeners = new ConcurrentDictionary<Guid, UdpListener>();
		_tcpip_listeners = new ConcurrentDictionary<Guid, TcpListener>();
		_tcpip_clients = new ConcurrentDictionary<Guid, TcpClientHandler>();
		_serial_ports = new ConcurrentDictionary<Guid, SerialPortHandler>();
		_mmio_data = new ConcurrentDictionary<Guid, clsMMIO>();
		ReceivedDataString += MultiMeterIO_ReceivedDataString;
		TransmittedData += MultiMeterIO_TransmittedData;
		ConnectorRunning += MultiMeterIO_ListenerRunning;
	}

	public static bool StartListeningUDP(clsMMIO mmio)
	{
		if (mmio == null)
		{
			return false;
		}
		if (mmio.Type != MMIOType.UDP_LISTENER)
		{
			return false;
		}
		if (!Common.IsIpv4Valid(mmio.IP, mmio.Port))
		{
			return false;
		}
		UdpListener udpListener;
		try
		{
			udpListener = new UdpListener(mmio.Guid, mmio.Type, mmio.IP, mmio.Port);
		}
		catch
		{
			return false;
		}
		udpListener.ReceivedDataString += delegate(Guid listenerGuid, string data)
		{
			ReceivedDataString?.Invoke(listenerGuid, data);
		};
		udpListener.ConnectorRunning += delegate(Guid listenerGuid, MMIOType type, bool running)
		{
			ConnectorRunning?.Invoke(listenerGuid, type, running);
		};
		udpListener.TransmittedData += delegate(Guid guid)
		{
			TransmittedData?.Invoke(guid);
		};
		if (_udp_listeners.TryAdd(mmio.Guid, udpListener))
		{
			udpListener.Start();
			return true;
		}
		return false;
	}

	public static bool StartListeningTCPIP(clsMMIO mmio)
	{
		if (mmio == null)
		{
			return false;
		}
		if (mmio.Type != MMIOType.TCPIP_LISTENER)
		{
			return false;
		}
		if (!Common.IsIpv4Valid(mmio.IP, mmio.Port))
		{
			return false;
		}
		TcpListener tcpListener;
		try
		{
			tcpListener = new TcpListener(mmio.Guid, mmio.Type, mmio.IP, mmio.Port);
		}
		catch
		{
			return false;
		}
		tcpListener.ReceivedDataString += delegate(Guid listenerGuid, string data)
		{
			ReceivedDataString?.Invoke(listenerGuid, data);
		};
		tcpListener.ConnectorRunning += delegate(Guid listenerGuid, MMIOType type, bool running)
		{
			ConnectorRunning?.Invoke(listenerGuid, type, running);
		};
		tcpListener.TransmittedData += delegate(Guid guid)
		{
			TransmittedData?.Invoke(guid);
		};
		if (_tcpip_listeners.TryAdd(mmio.Guid, tcpListener))
		{
			tcpListener.Start();
			return true;
		}
		return false;
	}

	public static bool StartTcpClient(clsMMIO mmio)
	{
		if (mmio == null)
		{
			return false;
		}
		if (mmio.Type != MMIOType.TCPIP_CLIENT)
		{
			return false;
		}
		if (!Common.IsIpv4Valid(mmio.IP, mmio.Port))
		{
			return false;
		}
		TcpClientHandler tcpClientHandler;
		try
		{
			tcpClientHandler = new TcpClientHandler(mmio.Guid, mmio.Type, mmio.IP, mmio.Port);
		}
		catch
		{
			return false;
		}
		tcpClientHandler.ReceivedDataString += delegate(Guid clientGuid, string data)
		{
			ReceivedDataString?.Invoke(clientGuid, data);
		};
		tcpClientHandler.ConnectorRunning += delegate(Guid clientGuid, MMIOType type, bool running)
		{
			ConnectorRunning?.Invoke(clientGuid, type, running);
		};
		tcpClientHandler.TransmittedData += delegate(Guid guid)
		{
			TransmittedData?.Invoke(guid);
		};
		if (_tcpip_clients.TryAdd(mmio.Guid, tcpClientHandler))
		{
			tcpClientHandler.Start();
			return true;
		}
		return false;
	}

	public static bool StartSerialPort(clsMMIO mmio)
	{
		if (mmio == null)
		{
			return false;
		}
		if (mmio.Type != MMIOType.SERIAL)
		{
			return false;
		}
		if (string.IsNullOrEmpty(mmio.ComPort) || mmio.BaudRate <= 0 || mmio.DataBits <= 0)
		{
			return false;
		}
		SerialPortHandler serialPortHandler;
		try
		{
			serialPortHandler = new SerialPortHandler(mmio.Guid, mmio.Type, mmio.ComPort, mmio.BaudRate, mmio.DataBits, mmio.StopBits, mmio.Parity);
		}
		catch
		{
			return false;
		}
		serialPortHandler.ReceivedDataString += delegate(Guid portGuid, string data)
		{
			ReceivedDataString?.Invoke(portGuid, data);
		};
		serialPortHandler.ConnectorRunning += delegate(Guid portGuid, MMIOType type, bool running)
		{
			ConnectorRunning?.Invoke(portGuid, type, running);
		};
		serialPortHandler.TransmittedData += delegate(Guid guid)
		{
			TransmittedData?.Invoke(guid);
		};
		if (_serial_ports.TryAdd(mmio.Guid, serialPortHandler))
		{
			serialPortHandler.Start();
			return true;
		}
		return false;
	}

	public static void StopConnection(Guid guid)
	{
		if (_udp_listeners.TryRemove(guid, out var value))
		{
			value.Stop();
		}
		if (_tcpip_listeners.TryRemove(guid, out var value2))
		{
			value2.Stop();
		}
		if (_tcpip_clients.TryRemove(guid, out var value3))
		{
			value3.Stop();
		}
		if (_serial_ports.TryRemove(guid, out var value4))
		{
			value4.Stop();
		}
	}

	public static void StopConnections()
	{
		foreach (KeyValuePair<Guid, UdpListener> udp_listener in _udp_listeners)
		{
			udp_listener.Value.Stop();
		}
		_udp_listeners.Clear();
		foreach (KeyValuePair<Guid, TcpListener> tcpip_listener in _tcpip_listeners)
		{
			tcpip_listener.Value.Stop();
		}
		_tcpip_listeners.Clear();
		foreach (KeyValuePair<Guid, TcpClientHandler> tcpip_client in _tcpip_clients)
		{
			tcpip_client.Value.Stop();
		}
		_tcpip_clients.Clear();
		foreach (KeyValuePair<Guid, SerialPortHandler> serial_port in _serial_ports)
		{
			serial_port.Value.Stop();
		}
		_serial_ports.Clear();
	}

	public static bool AlreadyConfigured(string ip, int port, MMIOType type)
	{
		foreach (KeyValuePair<Guid, clsMMIO> mmio_datum in _mmio_data)
		{
			clsMMIO value = mmio_datum.Value;
			if (value.IP.Equals(ip) && value.Port == port && value.Type == type)
			{
				return true;
			}
		}
		return false;
	}

	public static string GetSaveData()
	{
		string text = Common.SerializeToBase64(_mmio_data);
		text = text.Replace("/", "[backslash]");
		return "1|" + text;
	}

	public static bool RestoreSaveData2(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		try
		{
			StopConnections();
			string[] array = data.Split('|');
			if (array.Length != 2)
			{
				return false;
			}
			if (array[0] == "1")
			{
				_mmio_data = Common.DeserializeFromBase64<ConcurrentDictionary<Guid, clsMMIO>>(array[1].Replace("[backslash]", "/"));
			}
			foreach (KeyValuePair<Guid, clsMMIO> mmio_datum in _mmio_data)
			{
				clsMMIO value = mmio_datum.Value;
				if (value.Enabled)
				{
					value.StartConnection();
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool RestoreSaveData(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return true;
		}
		StopConnections();
		string[] array = data.Split('|');
		if (array.Length < 1)
		{
			return true;
		}
		bool flag = int.TryParse(array[0], out var result);
		if (flag)
		{
			Guid result2 = Guid.Empty;
			MMIODirection result3 = MMIODirection.BOTH;
			MMIOFormat result4 = MMIOFormat.JSON;
			MMIOFormat result5 = MMIOFormat.JSON;
			MMIOType result6 = MMIOType.UDP_LISTENER;
			MMIOTerminator result7 = MMIOTerminator.NONE;
			MMIOTerminator result8 = MMIOTerminator.NONE;
			int result9 = 0;
			bool result10 = false;
			int result11 = 0;
			int num = 0;
			int result12 = 0;
			int num2 = 0;
			for (int i = 0; i < result; i++)
			{
				clsMMIO clsMMIO2 = new clsMMIO();
				if (flag)
				{
					flag = Guid.TryParse(array[num2 + 1], out result2);
				}
				if (flag)
				{
					clsMMIO2.Guid = result2;
					flag = Enum.TryParse<MMIODirection>(array[num2 + 2], out result3);
				}
				if (flag)
				{
					clsMMIO2.Direction = result3;
					clsMMIO2.IP = array[num2 + 3];
					flag = int.TryParse(array[num2 + 4], out result9);
				}
				if (flag)
				{
					clsMMIO2.Port = result9;
					flag = Enum.TryParse<MMIOFormat>(array[num2 + 5], out result4);
				}
				if (flag)
				{
					clsMMIO2.FormatIn = result4;
					flag = Enum.TryParse<MMIOType>(array[num2 + 6], out result6);
				}
				if (flag)
				{
					clsMMIO2.Type = result6;
					flag = bool.TryParse(array[num2 + 7], out result10);
				}
				if (flag)
				{
					clsMMIO2.Enabled = result10;
					flag = Enum.TryParse<MMIOTerminator>(array[num2 + 8], out result7);
				}
				if (flag)
				{
					clsMMIO2.TerminatorIn = result7;
					flag = Enum.TryParse<MMIOFormat>(array[num2 + 9], out result5);
				}
				if (flag)
				{
					clsMMIO2.FormatOut = result5;
					flag = Enum.TryParse<MMIOTerminator>(array[num2 + 10], out result8);
				}
				if (flag)
				{
					clsMMIO2.TerminatorOut = result8;
					clsMMIO2.CustomTerminatorIn = array[num2 + 11].Replace("++><++", "|");
					clsMMIO2.CustomTerminatorOut = array[num2 + 12].Replace("++><++", "|");
					clsMMIO2.UdpEndpointIP = array[num2 + 13];
					flag = int.TryParse(array[num2 + 14], out result12);
				}
				if (flag)
				{
					clsMMIO2.UdpEndpointPort = result12;
				}
				if (flag)
				{
					num = 15;
					flag = int.TryParse(array[num2 + num], out result11);
					if (flag)
					{
						for (int j = 0; j < result11; j++)
						{
							string value = array[num2 + (num + 2) + j * 2].Replace("++><++", "|");
							Type type = clsMMIO2.DetermineType(value);
							object value2 = clsMMIO2.ConvertToType(value, type);
							flag = clsMMIO2.Variables().TryAdd(array[num2 + (num + 1) + j * 2], value2);
							if (!flag)
							{
								break;
							}
						}
					}
				}
				if (flag)
				{
					flag = _mmio_data.TryAdd(result2, clsMMIO2);
					if (!flag)
					{
						StopConnection(result2);
						return false;
					}
				}
				if (flag & result10)
				{
					clsMMIO2.StartConnection();
				}
				if (flag)
				{
					num2 += num + result11 * 2;
				}
			}
		}
		return flag;
	}

	public static bool AddMMIO(clsMMIO mmio)
	{
		if (_mmio_data.ContainsKey(mmio.Guid))
		{
			return false;
		}
		if (mmio == null)
		{
			return false;
		}
		bool flag = _mmio_data.TryAdd(mmio.Guid, mmio);
		if (!flag)
		{
			StopConnection(mmio.Guid);
			return false;
		}
		return flag;
	}

	public static bool RemoveMMIO(Guid guid)
	{
		bool flag = false;
		if (_mmio_data.ContainsKey(guid))
		{
			flag = _mmio_data.TryRemove(guid, out var _);
			if (flag)
			{
				StopConnection(guid);
			}
		}
		return flag;
	}

	public static void SendDataMMIO(Guid guid, string data)
	{
		if (_mmio_data.ContainsKey(guid))
		{
			_mmio_data[guid].EnqueueOutbound(data);
		}
	}

	public static Guid GuidfromFourChar(string fourChar)
	{
		foreach (KeyValuePair<Guid, clsMMIO> mmio_datum in _mmio_data)
		{
			clsMMIO value = mmio_datum.Value;
			if (value.FourChar == fourChar)
			{
				return value.Guid;
			}
		}
		return Guid.Empty;
	}

	private static void MultiMeterIO_ListenerRunning(Guid guid, MMIOType type, bool running)
	{
		if (Data.ContainsKey(guid))
		{
			Data[guid].Active = running;
		}
	}

	private static void MultiMeterIO_TransmittedData(Guid guid)
	{
	}

	[DebuggerHidden]
	public static bool IsValidXml(string xmlString)
	{
		try
		{
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
			xmlReaderSettings.XmlResolver = null;
			xmlReaderSettings.ValidationType = ValidationType.None;
			using (XmlReader xmlReader = XmlReader.Create(new StringReader(xmlString), xmlReaderSettings))
			{
				while (xmlReader.Read())
				{
				}
			}
			return true;
		}
		catch (XmlException)
		{
			return false;
		}
	}

	private static void MultiMeterIO_ReceivedDataString(Guid guid, string dataString)
	{
		char[] trimChars = new char[5] { ' ', '\n', '\r', '\t', '\0' };
		dataString = dataString.Trim(trimChars);
		if (string.IsNullOrWhiteSpace(dataString) || !Data.ContainsKey(guid))
		{
			return;
		}
		clsMMIO mmio = Data[guid];
		MMIOFormat formatIn = mmio.FormatIn;
		string fourChar = mmio.FourChar;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		switch (formatIn)
		{
		case MMIOFormat.JSON:
			try
			{
				parseJsonToken(JObject.Parse(dataString), fourChar, dictionary);
			}
			catch
			{
			}
			break;
		case MMIOFormat.XML:
			try
			{
				if (IsValidXml(dataString))
				{
					parseXMLElement(XElement.Parse(dataString), fourChar, dictionary);
				}
			}
			catch
			{
			}
			break;
		case MMIOFormat.RAW:
			try
			{
				string[] array = dataString.Split(':');
				int num = array.Length / 2;
				for (int i = 0; i < num; i++)
				{
					dictionary.Add(fourChar + "." + array[i * 2], array[i * 2 + 1]);
				}
			}
			catch
			{
			}
			break;
		}
		Parallel.ForEach(dictionary, delegate(KeyValuePair<string, string> kvp)
		{
			Type type = mmio.DetermineType(kvp.Value);
			object value = mmio.ConvertToType(kvp.Value, type);
			mmio.SetVariable(kvp.Key, value);
		});
	}

	public static void parseJsonToken(JToken token, string currentPath, Dictionary<string, string> keyValuePairs)
	{
		if (token is JValue)
		{
			if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float || token.Type == JTokenType.Boolean)
			{
				keyValuePairs[currentPath] = ((JValue)token).ToString(Newtonsoft.Json.Formatting.None);
			}
			else
			{
				keyValuePairs[currentPath] = ((JValue)token).ToString();
			}
			return;
		}
		if (token is JObject)
		{
			foreach (JProperty item in ((JObject)token).Properties())
			{
				parseJsonToken(item.Value, currentPath + "." + item.Name, keyValuePairs);
			}
			return;
		}
		if (token is JArray)
		{
			JArray jArray = (JArray)token;
			for (int i = 0; i < jArray.Count; i++)
			{
				parseJsonToken(jArray[i], $"{currentPath}[{i}]", keyValuePairs);
			}
		}
	}

	private static void parseXMLElement(XElement element, string currentPath, Dictionary<string, string> keyValuePairs)
	{
		foreach (XAttribute item in element.Attributes())
		{
			keyValuePairs[$"{currentPath}.{item.Name}"] = item.Value;
		}
		bool flag = false;
		foreach (XElement item2 in element.Elements())
		{
			flag = true;
			parseXMLElement(item2, $"{currentPath}.{item2.Name}", keyValuePairs);
		}
		if (!flag)
		{
			keyValuePairs[currentPath] = element.Value;
		}
	}
}
