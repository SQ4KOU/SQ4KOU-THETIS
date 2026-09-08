using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

public class BasicAudio
{
	public delegate void LoadComplededEventHandler(bool bLoadedOk);

	private SoundPlayer m_objPlayer;

	private bool m_bOkToPlay;

	private bool m_bLoading;

	private Thread m_objThread;

	public bool IsReady
	{
		get
		{
			return m_bOkToPlay;
		}
		set
		{
		}
	}

	public string SoundFile
	{
		get
		{
			if (m_objPlayer == null)
			{
				return "";
			}
			return m_objPlayer.SoundLocation;
		}
		set
		{
		}
	}

	private event LoadComplededEventHandler loadCompleted;

	public event LoadComplededEventHandler LoadCompletedEvent
	{
		add
		{
			loadCompleted += value;
		}
		remove
		{
			loadCompleted -= value;
		}
	}

	public BasicAudio()
	{
		try
		{
			m_objPlayer = new SoundPlayer();
		}
		catch (Exception ex)
		{
			m_objPlayer = null;
			MessageBox.Show("Unable to create SoundPlayer object (BasicAudio)\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144);
			return;
		}
		m_objPlayer.LoadCompleted += player_LoadCompleted;
		m_objPlayer.SoundLocationChanged += player_LocationChanged;
	}

	private void player_LocationChanged(object sender, EventArgs e)
	{
		m_bOkToPlay = false;
	}

	private void player_LoadCompleted(object sender, AsyncCompletedEventArgs e)
	{
		m_bOkToPlay = !e.Cancelled && e.Error == null;
		m_bLoading = false;
		loadCompleted?.Invoke(m_bOkToPlay);
	}

	public void LoadSound(string sFile)
	{
		if (m_objPlayer == null || m_bLoading || sFile == "")
		{
			return;
		}
		m_bOkToPlay = false;
		m_bLoading = true;
		m_objPlayer.SoundLocation = sFile;
		m_objPlayer.LoadTimeout = 1000;
		try
		{
			m_objPlayer.LoadAsync();
		}
		catch (FileNotFoundException)
		{
			m_bLoading = false;
			loadCompleted?.Invoke(bLoadedOk: false);
		}
		catch (TimeoutException)
		{
			m_bLoading = false;
			loadCompleted?.Invoke(bLoadedOk: false);
		}
	}

	public void Play()
	{
		if (m_objPlayer != null && m_bOkToPlay && (m_objThread == null || !m_objThread.IsAlive))
		{
			m_objThread = new Thread(playSound);
			m_objThread.Name = "Basic Audio Thread";
			m_objThread.Priority = ThreadPriority.BelowNormal;
			m_objThread.IsBackground = true;
			m_objThread.Start();
		}
	}

	public void Stop()
	{
		if (m_objPlayer == null)
		{
			return;
		}
		try
		{
			m_objPlayer.Stop();
		}
		catch
		{
		}
	}

	private void playSound()
	{
		if (m_objPlayer == null)
		{
			return;
		}
		try
		{
			m_objPlayer.Play();
		}
		catch
		{
		}
	}
}
