using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Thetis;

public class TCPIPcatServer
{
	public delegate void ClientConnected();

	public delegate void ClientDisconnected();

	public delegate void ClientError(SocketException se);

	public delegate void ServerError(SocketException se);

	public const string ID_G2V2_PANEL = "9f2b6c5a-4d7e-4c3b-9a21-3f8d0e6b12c4";

	public const string ID_ARIES_ATU = "2a7d4e1f-8b65-4a9c-b3d2-0f5e9c14a7e8";

	public const string ID_GANYMEDE = "c6e1f9a4-53b2-47d8-8c0e-2a7b5d14f963";

	public ClientConnected ClientConnectedHandlers;

	public ClientDisconnected ClientDisconnectedHandlers;

	public ClientError ClientErrorHandlers;

	public ServerError ServerErrorHandlers;

	private Console console;

	public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");

	public static int DEFAULT_PORT = 31001;

	public static IPEndPoint DEFAULT_IP_END_POINT = new IPEndPoint(DEFAULT_SERVER, DEFAULT_PORT);

	private TcpListener m_server;

	private bool m_stopServer;

	private bool m_stopPurging;

	private Thread m_serverThread;

	private Thread m_purgingThread;

	private List<TCPIPSocketListener> m_socketListenersList;

	private object m_objLocker = new object();

	private bool m_bSleepingInPurge;

	private frmLog _log;

	private string m_sLastError = "";

	private object m_WelcomeLock = new object();

	private bool m_bSendWelcome;

	public string LastError
	{
		get
		{
			string sLastError = m_sLastError;
			m_sLastError = "";
			return sLastError;
		}
	}

	public int ClientsConnected
	{
		get
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return 0;
			}
			int num = 0;
			lock (m_objLocker)
			{
				foreach (TCPIPSocketListener socketListeners in m_socketListenersList)
				{
					if (!socketListeners.IsDisconnected())
					{
						num++;
					}
				}
				return num;
			}
		}
	}

	public bool IsServerRunning => m_server != null;

	public bool SendWelcome
	{
		get
		{
			lock (m_WelcomeLock)
			{
				return m_bSendWelcome;
			}
		}
		set
		{
			lock (m_WelcomeLock)
			{
				m_bSendWelcome = value;
			}
		}
	}

	public frmLog LogForm => _log;

	public TCPIPcatServer()
	{
		Init(DEFAULT_IP_END_POINT);
	}

	public TCPIPcatServer(IPAddress serverIP)
	{
		Init(new IPEndPoint(serverIP, DEFAULT_PORT));
	}

	public TCPIPcatServer(int port)
	{
		Init(new IPEndPoint(DEFAULT_SERVER, port));
	}

	public TCPIPcatServer(IPAddress serverIP, int port)
	{
		Init(new IPEndPoint(serverIP, port));
	}

	public TCPIPcatServer(IPEndPoint ipNport)
	{
		Init(ipNport);
	}

	~TCPIPcatServer()
	{
		StopServer();
		if (_log != null)
		{
			_log.Close();
			_log = null;
		}
	}

	private void Init(IPEndPoint ipNport)
	{
		try
		{
			if (_log != null)
			{
				_log = null;
			}
			_log = new frmLog();
			m_server = new TcpListener(ipNport);
		}
		catch (Exception)
		{
			m_server = null;
		}
	}

	public void StartServer(Console c, bool bTCPIPcatWelcomeMessage = true)
	{
		if (m_server != null)
		{
			console = c;
			m_bSendWelcome = bTCPIPcatWelcomeMessage;
			m_socketListenersList = new List<TCPIPSocketListener>();
			try
			{
				m_server.Start();
				m_serverThread = new Thread(ServerThreadStart);
				m_serverThread.Priority = ThreadPriority.BelowNormal;
				m_serverThread.Name = "TCPIP cat server Thread";
				m_serverThread.Start();
				m_purgingThread = new Thread(PurgingThreadStart);
				m_purgingThread.Priority = ThreadPriority.Lowest;
				m_purgingThread.Name = "TCPIP cat purging Thread";
				m_purgingThread.Start();
			}
			catch (SocketException ex)
			{
				m_sLastError = ex.Message;
				StopServer();
				ServerErrorHandlers?.Invoke(ex);
			}
		}
	}

	public void SendToClients(string sMsg, List<string> id_limit = null)
	{
		if (m_server == null)
		{
			return;
		}
		try
		{
			lock (m_objLocker)
			{
				foreach (TCPIPSocketListener socketListeners in m_socketListenersList)
				{
					socketListeners.SendData(sMsg, id_limit);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	public void StopServer()
	{
		if (m_server == null)
		{
			return;
		}
		m_stopServer = true;
		try
		{
			m_server.Stop();
		}
		catch
		{
		}
		if (m_serverThread != null)
		{
			m_serverThread.Join(50);
			if (m_serverThread.IsAlive)
			{
				m_serverThread.Abort();
			}
			m_serverThread = null;
		}
		m_stopPurging = true;
		if (m_purgingThread != null)
		{
			if (!m_bSleepingInPurge)
			{
				m_purgingThread.Join(500);
			}
			if (m_purgingThread.IsAlive)
			{
				m_purgingThread.Abort();
			}
			m_purgingThread = null;
		}
		m_server = null;
		StopAllSocketListers();
	}

	private void StopAllSocketListers()
	{
		lock (m_objLocker)
		{
			foreach (TCPIPSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.StopSocketListener();
				socketListeners.ClientConnectedHandlers = (TCPIPSocketListener.ClientConnected)Delegate.Remove(socketListeners.ClientConnectedHandlers, new TCPIPSocketListener.ClientConnected(ClientConnectedHandler));
				socketListeners.ClientDisconnectedHandlers = (TCPIPSocketListener.ClientDisconnected)Delegate.Remove(socketListeners.ClientDisconnectedHandlers, new TCPIPSocketListener.ClientDisconnected(ClientDisconnectedHandler));
				socketListeners.ClientErrorHandlers = (TCPIPSocketListener.ClientError)Delegate.Remove(socketListeners.ClientErrorHandlers, new TCPIPSocketListener.ClientError(ClientErrorHandler));
			}
			m_socketListenersList.Clear();
			m_socketListenersList = null;
		}
	}

	private void ServerThreadStart()
	{
		TCPIPSocketListener tCPIPSocketListener = null;
		bool flag = false;
		while (!m_stopServer)
		{
			try
			{
				flag = false;
				tCPIPSocketListener = new TCPIPSocketListener(m_server.AcceptSocket(), console, this);
				lock (m_objLocker)
				{
					m_socketListenersList.Add(tCPIPSocketListener);
				}
				TCPIPSocketListener tCPIPSocketListener2 = tCPIPSocketListener;
				tCPIPSocketListener2.ClientConnectedHandlers = (TCPIPSocketListener.ClientConnected)Delegate.Combine(tCPIPSocketListener2.ClientConnectedHandlers, new TCPIPSocketListener.ClientConnected(ClientConnectedHandler));
				TCPIPSocketListener tCPIPSocketListener3 = tCPIPSocketListener;
				tCPIPSocketListener3.ClientDisconnectedHandlers = (TCPIPSocketListener.ClientDisconnected)Delegate.Combine(tCPIPSocketListener3.ClientDisconnectedHandlers, new TCPIPSocketListener.ClientDisconnected(ClientDisconnectedHandler));
				TCPIPSocketListener tCPIPSocketListener4 = tCPIPSocketListener;
				tCPIPSocketListener4.ClientErrorHandlers = (TCPIPSocketListener.ClientError)Delegate.Combine(tCPIPSocketListener4.ClientErrorHandlers, new TCPIPSocketListener.ClientError(ClientErrorHandler));
				flag = true;
				tCPIPSocketListener.StartSocketListener();
			}
			catch (SocketException ex)
			{
				if (flag && tCPIPSocketListener != null)
				{
					TCPIPSocketListener tCPIPSocketListener5 = tCPIPSocketListener;
					tCPIPSocketListener5.ClientConnectedHandlers = (TCPIPSocketListener.ClientConnected)Delegate.Remove(tCPIPSocketListener5.ClientConnectedHandlers, new TCPIPSocketListener.ClientConnected(ClientConnectedHandler));
					TCPIPSocketListener tCPIPSocketListener6 = tCPIPSocketListener;
					tCPIPSocketListener6.ClientDisconnectedHandlers = (TCPIPSocketListener.ClientDisconnected)Delegate.Remove(tCPIPSocketListener6.ClientDisconnectedHandlers, new TCPIPSocketListener.ClientDisconnected(ClientDisconnectedHandler));
					TCPIPSocketListener tCPIPSocketListener7 = tCPIPSocketListener;
					tCPIPSocketListener7.ClientErrorHandlers = (TCPIPSocketListener.ClientError)Delegate.Remove(tCPIPSocketListener7.ClientErrorHandlers, new TCPIPSocketListener.ClientError(ClientErrorHandler));
				}
				m_stopServer = true;
				m_sLastError = ex.Message;
				ServerErrorHandlers?.Invoke(ex);
			}
		}
	}

	private void PurgingThreadStart()
	{
		while (!m_stopPurging)
		{
			List<TCPIPSocketListener> list = new List<TCPIPSocketListener>();
			lock (m_objLocker)
			{
				foreach (TCPIPSocketListener socketListeners in m_socketListenersList)
				{
					if (socketListeners.IsMarkedForDeletion())
					{
						list.Add(socketListeners);
						socketListeners.StopSocketListener();
						socketListeners.ClientConnectedHandlers = (TCPIPSocketListener.ClientConnected)Delegate.Remove(socketListeners.ClientConnectedHandlers, new TCPIPSocketListener.ClientConnected(ClientConnectedHandler));
						socketListeners.ClientDisconnectedHandlers = (TCPIPSocketListener.ClientDisconnected)Delegate.Remove(socketListeners.ClientDisconnectedHandlers, new TCPIPSocketListener.ClientDisconnected(ClientDisconnectedHandler));
						socketListeners.ClientErrorHandlers = (TCPIPSocketListener.ClientError)Delegate.Remove(socketListeners.ClientErrorHandlers, new TCPIPSocketListener.ClientError(ClientErrorHandler));
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					m_socketListenersList.Remove(list[i]);
				}
			}
			list = null;
			m_bSleepingInPurge = true;
			Thread.Sleep(5000);
			m_bSleepingInPurge = false;
		}
	}

	private void ClientConnectedHandler()
	{
		ClientConnectedHandlers?.Invoke();
	}

	private void ClientDisconnectedHandler()
	{
		ClientDisconnectedHandlers?.Invoke();
	}

	private void ClientErrorHandler(SocketException se)
	{
		m_sLastError = se.Message;
		ClientErrorHandlers?.Invoke(se);
	}

	public void ShowLog()
	{
		if (_log != null)
		{
			_log.ShowWithTitle("TCPIPcat");
		}
	}

	public void CloseLog()
	{
		if (_log != null)
		{
			_log.Hide();
		}
	}
}
