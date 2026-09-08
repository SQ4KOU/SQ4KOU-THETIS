using System;
using System.Threading;
using System.Windows.Forms;

namespace Thetis;

internal static class SingleInstance
{
	private const string MUTEX_NAME = "Global\\Thetis_7F1F9E7F-6C3E-4F3E-9E4C-8E0A3C6E8C11";

	private static Mutex _mutex;

	private static bool _owns_mutex;

	public static bool CheckAndPrompt()
	{
		try
		{
			_mutex = new Mutex(initiallyOwned: true, "Global\\Thetis_7F1F9E7F-6C3E-4F3E-9E4C-8E0A3C6E8C11", out var createdNew);
			if (createdNew)
			{
				_owns_mutex = true;
				return true;
			}
			try
			{
				if (_mutex.WaitOne(TimeSpan.Zero))
				{
					_owns_mutex = true;
					return true;
				}
			}
			catch (AbandonedMutexException)
			{
				_owns_mutex = true;
				return true;
			}
			if (MessageBox.Show("There is another Thetis instance running.\nAre you sure you want to continue?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144) == DialogResult.No)
			{
				return false;
			}
			return true;
		}
		catch (Exception ex2)
		{
			if (MessageBox.Show("There was an issue trying to determine if another Thetis instance is running.\nAre you sure you want to continue?\n\n" + ex2.GetType().Name + ": " + ex2.Message, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (MessageBoxOptions)262144) == DialogResult.No)
			{
				return false;
			}
			return true;
		}
	}

	public static void Release()
	{
		if (_owns_mutex && _mutex != null)
		{
			try
			{
				_mutex.ReleaseMutex();
			}
			catch
			{
			}
			_mutex.Dispose();
			_mutex = null;
			_owns_mutex = false;
		}
	}
}
