using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Thetis;

internal class DumpCap
{
	private static Console m_objConsole;

	private static Thread m_objRunThread;

	private static int m_nTimeOut;

	private static bool m_bEnabled = false;

	private static bool m_bKillOnNegativeSeqOnly = true;

	private static int m_nInterface = 1;

	private static string m_sWireSharkPath = "";

	private static int m_nFileSizeKB = 10000;

	private static int m_nNumberOfFiles = 2;

	private static int m_nProcessID = -1;

	private static bool m_bClearFolderOnRestart = true;

	public static bool Enabled
	{
		get
		{
			return m_bEnabled;
		}
		set
		{
			m_bEnabled = value;
			if (m_bEnabled)
			{
				StartDumpcap(m_nTimeOut);
			}
			else
			{
				StopDumpcap();
			}
		}
	}

	public static int Interface
	{
		get
		{
			return m_nInterface;
		}
		set
		{
			m_nInterface = value;
			restartDumpcap();
		}
	}

	public static string WireSharkPath
	{
		get
		{
			return m_sWireSharkPath;
		}
		set
		{
			m_sWireSharkPath = value;
			restartDumpcap();
		}
	}

	public static int FileSizeKB
	{
		get
		{
			return m_nFileSizeKB;
		}
		set
		{
			m_nFileSizeKB = value;
			restartDumpcap();
		}
	}

	public static int NumberOfFiles
	{
		get
		{
			return m_nNumberOfFiles;
		}
		set
		{
			m_nNumberOfFiles = value;
			restartDumpcap();
		}
	}

	public static bool ClearFolderOnRestart
	{
		get
		{
			return m_bClearFolderOnRestart;
		}
		set
		{
			m_bClearFolderOnRestart = value;
		}
	}

	public static bool KillOnNegativeSeqOnly
	{
		get
		{
			return m_bKillOnNegativeSeqOnly;
		}
		set
		{
			m_bKillOnNegativeSeqOnly = value;
		}
	}

	private static string workingFolder
	{
		get
		{
			if (m_objConsole == null)
			{
				return "";
			}
			return m_objConsole.AppDataPath + "dumpcap\\";
		}
	}

	public static bool DumpCapExists()
	{
		return File.Exists(m_sWireSharkPath + "\\dumpcap.exe ");
	}

	private static void restartDumpcap()
	{
		if (isDumpcapRunning())
		{
			StopDumpcap();
			StartDumpcap(m_nTimeOut);
		}
	}

	public static void Initalise(Console c)
	{
		m_objConsole = c;
	}

	public static void ClearDumpFolder()
	{
		if (m_objConsole == null || workingFolder == "" || !Directory.Exists(workingFolder))
		{
			return;
		}
		FileInfo[] files = new DirectoryInfo(workingFolder).GetFiles("*.pcapng");
		foreach (FileInfo fileInfo in files)
		{
			try
			{
				fileInfo.Delete();
			}
			catch
			{
			}
		}
	}

	private static void dumpcapGO()
	{
		if (!DumpCapExists() || m_objConsole == null || workingFolder == "")
		{
			return;
		}
		int num = 0;
		while (isDumpcapRunning() && num < m_nTimeOut)
		{
			Thread.Sleep(1);
			num++;
		}
		if (isDumpcapRunning())
		{
			return;
		}
		try
		{
			string arguments = $"-i {m_nInterface} -b filesize:{m_nFileSizeKB} -b files:{m_nNumberOfFiles} -w dumpcap_thetis.pcapng";
			if (!Directory.Exists(workingFolder))
			{
				Directory.CreateDirectory(workingFolder);
			}
			using Process process = new Process();
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.WorkingDirectory = workingFolder;
			process.StartInfo.FileName = m_sWireSharkPath + "\\dumpcap.exe ";
			process.StartInfo.Arguments = arguments;
			process.StartInfo.CreateNoWindow = true;
			process.Start();
			m_nProcessID = process.Id;
		}
		catch
		{
		}
	}

	public static void StartDumpcap(int nTimeOut)
	{
		if (m_bEnabled && !isDumpcapRunning() && (m_objRunThread == null || !m_objRunThread.IsAlive))
		{
			m_nTimeOut = nTimeOut;
			m_objRunThread = new Thread(dumpcapGO)
			{
				Name = "Dumpcap start Thread",
				Priority = ThreadPriority.AboveNormal,
				IsBackground = true
			};
			m_objRunThread.Start();
		}
	}

	public static void StopDumpcap()
	{
		if (!isDumpcapRunning())
		{
			return;
		}
		Process[] processesByName = Process.GetProcessesByName("dumpcap");
		foreach (Process process in processesByName)
		{
			if (process.Id == m_nProcessID)
			{
				try
				{
					process.Kill();
					m_nProcessID = -1;
					break;
				}
				catch
				{
					break;
				}
			}
		}
	}

	private static bool isDumpcapRunning()
	{
		if (m_nProcessID == -1)
		{
			return false;
		}
		bool result = false;
		Process[] processesByName = Process.GetProcessesByName("dumpcap");
		for (int i = 0; i < processesByName.Length; i++)
		{
			if (processesByName[i].Id == m_nProcessID)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static void ShowAppPathFolder()
	{
		if (m_objConsole == null || workingFolder == "" || !Directory.Exists(workingFolder))
		{
			return;
		}
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = workingFolder,
				UseShellExecute = true,
				Verb = "open"
			});
		}
		catch
		{
		}
	}
}
