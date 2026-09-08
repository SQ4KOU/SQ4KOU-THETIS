using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Thetis;

public class TCPIPSocketListener
{
	public delegate void ClientConnected();

	public delegate void ClientDisconnected();

	public delegate void ClientError(SocketException se);

	private sealed class BroadcastItem
	{
		public string Message;

		public List<string> IdLimit;
	}

	public ClientConnected ClientConnectedHandlers;

	public ClientDisconnected ClientDisconnectedHandlers;

	public ClientError ClientErrorHandlers;

	private Console console;

	private TCPIPcatServer m_server;

	private Socket m_clientSocket;

	private bool m_stopClient;

	private bool m_disconnected;

	private Thread m_clientListenerThread;

	private bool m_markedForDeletion;

	private StringBuilder m_oneLineBuf = new StringBuilder();

	private DateTime m_lastReceiveDateTime;

	private DateTime m_lastSendDateTime;

	private DateTime m_currentReceiveDateTime;

	private DateTime m_currentSendDateTime;

	private ConcurrentQueue<BroadcastItem> _broadcast = new ConcurrentQueue<BroadcastItem>();

	private readonly object _client_ids_lock = new object();

	private List<string> _client_ids;

	public TCPIPSocketListener(Socket clientSocket, Console c, TCPIPcatServer server)
	{
		console = c;
		m_server = server;
		m_clientSocket = clientSocket;
	}

	~TCPIPSocketListener()
	{
		StopSocketListener();
		m_server = null;
	}

	public void StartSocketListener()
	{
		if (m_clientSocket != null)
		{
			m_clientListenerThread = new Thread(SocketListenerThreadStart);
			m_clientListenerThread.Name = "TCPIP cat clientListener Thread";
			m_clientListenerThread.Start();
		}
	}

	private void SocketListenerThreadStart()
	{
		int num = 0;
		m_lastReceiveDateTime = DateTime.UtcNow;
		m_lastSendDateTime = DateTime.UtcNow;
		m_currentReceiveDateTime = DateTime.UtcNow;
		m_currentSendDateTime = DateTime.UtcNow;
		Timer timer = new Timer(checkClientCommInterval, null, 30000, 30000);
		ClientConnectedHandlers?.Invoke();
		if (m_server != null && m_server.SendWelcome && console != null)
		{
			internal_send_data("#Thetis TCP/IP Cat - " + console.VersionWithoutFW.Replace(";", "") + "#;");
		}
		while (!m_stopClient)
		{
			try
			{
				bool flag = false;
				int available = m_clientSocket.Available;
				if (available > 0)
				{
					byte[] array = new byte[available];
					num = m_clientSocket.Receive(array);
					if (num > 0)
					{
						m_currentReceiveDateTime = DateTime.UtcNow;
						ParseReceiveBuffer(array, num);
						flag = true;
					}
				}
				else
				{
					Queue<BroadcastItem> queue = new Queue<BroadcastItem>();
					BroadcastItem result;
					while (_broadcast.TryDequeue(out result))
					{
						queue.Enqueue(result);
					}
					while (queue.Count > 0)
					{
						BroadcastItem broadcastItem = queue.Dequeue();
						if (shouldSend(broadcastItem.IdLimit))
						{
							internal_send_data(broadcastItem.Message);
						}
						flag = true;
					}
				}
				if (!flag)
				{
					Thread.Sleep(50);
				}
			}
			catch (SocketException se)
			{
				m_stopClient = true;
				m_markedForDeletion = true;
				ClientErrorHandlers?.Invoke(se);
			}
		}
		timer.Change(-1, -1);
		timer = null;
		m_disconnected = true;
		ClientDisconnectedHandlers?.Invoke();
	}

	private bool shouldSend(List<string> id_limit)
	{
		if (id_limit == null || id_limit.Count == 0)
		{
			return true;
		}
		List<string> client_ids;
		lock (_client_ids_lock)
		{
			client_ids = _client_ids;
		}
		if (client_ids == null)
		{
			return false;
		}
		bool flag = false;
		for (int i = 0; i < id_limit.Count; i++)
		{
			if (!string.IsNullOrWhiteSpace(id_limit[i]))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return false;
		}
		for (int j = 0; j < client_ids.Count; j++)
		{
			if (string.IsNullOrWhiteSpace(client_ids[j]))
			{
				continue;
			}
			for (int k = 0; k < id_limit.Count; k++)
			{
				string text = id_limit[k];
				if (!string.IsNullOrWhiteSpace(text) && string.Equals(text, client_ids[j], StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void addClientId(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return;
		}
		lock (_client_ids_lock)
		{
			if (_client_ids == null)
			{
				_client_ids = new List<string>();
			}
			for (int i = 0; i < _client_ids.Count; i++)
			{
				if (string.Equals(_client_ids[i], id, StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}
			_client_ids.Add(id);
		}
	}

	public void removeClientId(string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return;
		}
		lock (_client_ids_lock)
		{
			if (_client_ids == null)
			{
				return;
			}
			for (int num = _client_ids.Count - 1; num >= 0; num--)
			{
				if (string.Equals(_client_ids[num], id, StringComparison.OrdinalIgnoreCase))
				{
					_client_ids.RemoveAt(num);
				}
			}
			if (_client_ids.Count == 0)
			{
				_client_ids = null;
			}
		}
	}

	public void StopSocketListener()
	{
		if (m_clientSocket != null)
		{
			m_stopClient = true;
			m_clientSocket.Close();
			m_clientListenerThread.Join(50);
			if (m_clientListenerThread.IsAlive)
			{
				m_clientListenerThread.Abort();
				m_disconnected = true;
				ClientDisconnectedHandlers?.Invoke();
			}
			m_clientListenerThread = null;
			m_clientSocket = null;
			m_markedForDeletion = true;
		}
	}

	public bool IsMarkedForDeletion()
	{
		return m_markedForDeletion;
	}

	public bool IsDisconnected()
	{
		return m_disconnected;
	}

	private void ParseReceiveBuffer(byte[] byteBuffer, int size)
	{
		string value = Encoding.ASCII.GetString(byteBuffer, 0, size);
		m_oneLineBuf.Append(value);
		m_oneLineBuf.Replace(Environment.NewLine, "");
		string text = m_oneLineBuf.ToString();
		for (int num = text.IndexOf(";"); num > -1; num = text.IndexOf(";"))
		{
			string sInboundCatCommand = text.Substring(0, num + 1);
			processClientData(sInboundCatCommand);
			m_oneLineBuf.Remove(0, num + 1);
			text = m_oneLineBuf.ToString();
		}
		if (m_oneLineBuf.Length > 255)
		{
			m_oneLineBuf.Clear();
		}
	}

	private void processClientData(string sInboundCatCommand)
	{
		if (m_server != null && m_server.LogForm != null)
		{
			m_server.LogForm.Log(bIn: true, sInboundCatCommand);
		}
		sInboundCatCommand = sInboundCatCommand.Trim();
		Guid result;
		if (sInboundCatCommand.StartsWith("ZZGA", StringComparison.OrdinalIgnoreCase))
		{
			bool flag = false;
			if (sInboundCatCommand.Length >= 40)
			{
				string text = sInboundCatCommand.Substring(4, 36).ToLower();
				if (Guid.TryParse(text, out result))
				{
					addClientId(text);
					internal_send_data("ZZGA" + text + ";");
					flag = true;
				}
			}
			if (!flag)
			{
				internal_send_data("?;");
			}
		}
		else if (sInboundCatCommand.StartsWith("ZZGR", StringComparison.OrdinalIgnoreCase))
		{
			bool flag2 = false;
			if (sInboundCatCommand.Length >= 40)
			{
				string text2 = sInboundCatCommand.Substring(4, 36).ToLower();
				if (Guid.TryParse(text2, out result))
				{
					removeClientId(text2);
					internal_send_data("ZZGR" + text2 + ";");
					flag2 = true;
				}
			}
			if (!flag2)
			{
				internal_send_data("?;");
			}
		}
		else
		{
			string text3 = console.ThreadSafeCatParse(sInboundCatCommand);
			if (text3.Length > 0)
			{
				internal_send_data(text3);
			}
		}
	}

	private void internal_send_data(string oneLine)
	{
		if (m_clientSocket == null)
		{
			return;
		}
		try
		{
			if (m_clientSocket.Connected)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(oneLine);
				m_clientSocket.Send(bytes);
				if (m_server != null && m_server.LogForm != null)
				{
					m_server.LogForm.Log(bIn: false, oneLine);
				}
				m_currentSendDateTime = DateTime.UtcNow;
			}
		}
		catch (SocketException se)
		{
			m_stopClient = true;
			m_markedForDeletion = true;
			ClientErrorHandlers?.Invoke(se);
		}
	}

	public void SendData(string data, List<string> id_limit = null)
	{
		if (m_clientSocket != null)
		{
			string[] array = data.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				BroadcastItem broadcastItem = new BroadcastItem();
				broadcastItem.Message = array[i] + ";";
				broadcastItem.IdLimit = id_limit;
				_broadcast.Enqueue(broadcastItem);
			}
		}
	}

	private void checkClientCommInterval(object o)
	{
		bool flag = false;
		bool flag2 = false;
		if (m_lastReceiveDateTime.Equals(m_currentReceiveDateTime))
		{
			flag = true;
		}
		else
		{
			m_lastReceiveDateTime = m_currentReceiveDateTime;
		}
		if (m_lastSendDateTime.Equals(m_currentSendDateTime))
		{
			flag2 = true;
		}
		else
		{
			m_lastSendDateTime = m_currentSendDateTime;
		}
		if (flag & flag2)
		{
			StopSocketListener();
		}
	}
}
