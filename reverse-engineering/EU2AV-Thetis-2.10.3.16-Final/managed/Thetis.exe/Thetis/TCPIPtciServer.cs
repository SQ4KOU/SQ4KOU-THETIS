using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Thetis;

public class TCPIPtciServer
{
	public delegate void ClientConnected();

	public delegate void ClientDisconnected();

	public delegate void ClientError(SocketException se);

	public delegate void ServerError(SocketException se);

	private sealed class TCICWController : IDisposable
	{
		private sealed class CWTxSegment
		{
			public string Text;

			public int SpeedWpm;
		}

		private sealed class CWTextParseResult
		{
			public readonly List<CWTxSegment> Segments = new List<CWTxSegment>();

			public int FinalSpeedWpm;

			public bool UsedInlineSpeedChanges;
		}

		private sealed class CWTxOperation
		{
			public readonly List<CWTxSegment> Segments = new List<CWTxSegment>();

			public int Rx;

			public int NextSegmentIndex;

			public int ActiveSegmentIndex = -1;

			public int CallsignSegmentIndex = -1;

			public string CallsignBase = string.Empty;

			public int BaseSpeedWpm;

			public bool RestoreBaseSpeed;

			public bool EmptyNotified;

			public bool CallsignNotified;

			public TCPIPtciSocketListener Owner;
		}

		private const int DirectKeyerWatchdogMs = 3000;

		private readonly TCPIPtciServer _server;

		private readonly object _lockObj = new object();

		private readonly Timer _pollTimer;

		private readonly AutoResetEvent _keyerScheduleEvent;

		private readonly Thread _keyerSchedulerThread;

		private readonly Stopwatch _keyerStopwatch;

		private readonly Queue<CWTxOperation> _pendingOperations = new Queue<CWTxOperation>();

		private CWTxOperation _activeOperation;

		private readonly bool[] _terminalEnabledByRx = new bool[2];

		private TCPIPtciSocketListener _currentOwner;

		private bool _terminalTciPttAsserted;

		private int _terminalTciPttRx = -1;

		private bool _releaseTerminalTciPttWhenIdle;

		private bool _keyerPressed;

		private bool _keyerReleasePending;

		private long _keyerPressedAtTicks = -1L;

		private long _keyerReleaseAtTicks = -1L;

		private int _keyerRx = -1;

		private bool _keyerAssertedMox;

		private bool _disposed;

		public TCICWController(TCPIPtciServer server)
		{
			_server = server;
			_pollTimer = new Timer(PollCallback, null, 50, 50);
			_keyerScheduleEvent = new AutoResetEvent(initialState: false);
			_keyerStopwatch = Stopwatch.StartNew();
			_keyerSchedulerThread = new Thread(KeyerSchedulerThreadProc)
			{
				IsBackground = true,
				Name = "TCI CW Keyer",
				Priority = ThreadPriority.Highest
			};
			_keyerSchedulerThread.Start();
			setDirectKeyerState(pressed: false);
		}

		public void Dispose()
		{
			bool flag;
			bool flag2;
			bool terminalTciPttAsserted;
			lock (_lockObj)
			{
				_disposed = true;
				_pendingOperations.Clear();
				_activeOperation = null;
				flag = _keyerPressed || _keyerReleasePending;
				_keyerPressed = false;
				_keyerReleasePending = false;
				_keyerPressedAtTicks = -1L;
				_keyerReleaseAtTicks = -1L;
				_keyerRx = -1;
				flag2 = captureDirectKeyerMoxReleaseLocked();
				terminalTciPttAsserted = _terminalTciPttAsserted;
				_terminalTciPttAsserted = false;
				_terminalTciPttRx = -1;
				_currentOwner = null;
			}
			_keyerScheduleEvent.Set();
			try
			{
				_keyerSchedulerThread?.Join(250);
			}
			catch
			{
			}
			_pollTimer?.Dispose();
			if (flag)
			{
				setDirectKeyerState(pressed: false);
			}
			if (flag2)
			{
				releaseDirectKeyerMox();
			}
			if (terminalTciPttAsserted)
			{
				InvokeOnConsole(delegate(Console c)
				{
					c.TCIPTT = false;
				});
			}
			_keyerScheduleEvent?.Dispose();
		}

		public int GetMacroSpeed()
		{
			lock (_lockObj)
			{
				if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
				{
					return _activeOperation.BaseSpeedWpm;
				}
			}
			return InvokeOnConsole((Console c) => c.CWXForm.WPM, 30);
		}

		public void SetMacroSpeed(int wpm)
		{
			int clamped = clampMacroSpeed(wpm);
			lock (_lockObj)
			{
				if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
				{
					_activeOperation.BaseSpeedWpm = clamped;
				}
			}
			InvokeOnConsole(delegate(Console c)
			{
				c.CWXForm.WPM = clamped;
			});
		}

		private void SetMacroSpeedSilently(int wpm)
		{
			Interlocked.Increment(ref _server.m_cwInternalMacroSpeedUpdates);
			try
			{
				InvokeOnConsole(delegate(Console c)
				{
					c.CWXForm.WPM = clampMacroSpeed(wpm);
				});
			}
			finally
			{
				Interlocked.Decrement(ref _server.m_cwInternalMacroSpeedUpdates);
			}
		}

		public int GetMacroDelayMs()
		{
			return InvokeOnConsole((Console c) => c.CWXForm.PTTDelayMs, 0);
		}

		public void SetMacroDelayMs(int delayMs)
		{
			InvokeOnConsole(delegate(Console c)
			{
				c.CWXForm.PTTDelayMs = Math.Max(0, delayMs);
			});
		}

		public int GetKeyerSpeed()
		{
			return InvokeOnConsole((Console c) => c.CATCWSpeed, 30);
		}

		public void SetKeyerSpeed(int wpm)
		{
			InvokeOnConsole(delegate(Console c)
			{
				c.CATCWSpeed = Math.Max(1, Math.Min(60, wpm));
			});
		}

		public void IncreaseMacroSpeed(int amount)
		{
			SetMacroSpeed(GetMacroSpeed() + Math.Max(0, amount));
		}

		public void DecreaseMacroSpeed(int amount)
		{
			SetMacroSpeed(GetMacroSpeed() - Math.Max(0, amount));
		}

		public void SetTerminalEnabled(TCPIPtciSocketListener owner, int rx, bool enabled)
		{
			lock (_lockObj)
			{
				if (rx < 0 || rx > 1)
				{
					return;
				}
				if (enabled)
				{
					if (!tryAcquireOwnershipLocked(owner))
					{
						return;
					}
				}
				else if (!isCurrentOwnerLocked(owner))
				{
					return;
				}
				_terminalEnabledByRx[rx] = enabled;
				if (enabled)
				{
					if (_activeOperation != null && _activeOperation.Rx == rx)
					{
						ensureTerminalTciPttLocked();
					}
					_releaseTerminalTciPttWhenIdle = false;
				}
				else if (_activeOperation != null && _activeOperation.Rx == rx)
				{
					_releaseTerminalTciPttWhenIdle = true;
				}
				else if (_activeOperation == null && _terminalTciPttAsserted && _terminalTciPttRx == rx)
				{
					releaseTerminalTciPttIfOwnedLocked();
				}
				releaseOwnershipIfIdleLocked();
			}
		}

		public void SendMacro(TCPIPtciSocketListener owner, int rx, string text)
		{
			lock (_lockObj)
			{
				if (isCwTargetAvailableLocked(rx) && tryAcquireOwnershipLocked(owner) && !_keyerPressed)
				{
					CWTxOperation cWTxOperation = buildMacroOperation(rx, text);
					cWTxOperation.Owner = owner;
					_pendingOperations.Enqueue(cWTxOperation);
					startNextOperationLocked();
				}
			}
		}

		public void SendMessage(TCPIPtciSocketListener owner, int rx, string prefix, string callsign, string suffix)
		{
			lock (_lockObj)
			{
				if (isCwTargetAvailableLocked(rx) && tryAcquireOwnershipLocked(owner) && !_keyerPressed)
				{
					CWTxOperation cWTxOperation = buildMessageOperation(rx, prefix, callsign, suffix);
					cWTxOperation.Owner = owner;
					_pendingOperations.Enqueue(cWTxOperation);
					startNextOperationLocked();
				}
			}
		}

		public void HandleKeyer(TCPIPtciSocketListener owner, int rx, bool pressed, int durationMs)
		{
			lock (_lockObj)
			{
				if (pressed)
				{
					if (!tryAcquireOwnershipLocked(owner) || _activeOperation != null || _pendingOperations.Count > 0)
					{
						return;
					}
					if (_keyerReleasePending)
					{
						releaseKeyerLocked();
					}
					if (!_keyerPressed)
					{
						if (!selectCwTargetLocked(rx) || !isCWModeLocked())
						{
							releaseOwnershipIfIdleLocked();
							return;
						}
						if (!beginDirectKeyerLocked())
						{
							releaseOwnershipIfIdleLocked();
							return;
						}
						_keyerPressed = true;
						_keyerReleasePending = false;
						_keyerPressedAtTicks = _keyerStopwatch.ElapsedTicks;
						_keyerReleaseAtTicks = -1L;
						_keyerRx = rx;
					}
				}
				else if (isCurrentOwnerLocked(owner) && _keyerPressed)
				{
					scheduleKeyerReleaseLocked(durationMs);
				}
			}
		}

		public void UpdatePendingCallsign(TCPIPtciSocketListener owner, string callsign)
		{
			lock (_lockObj)
			{
				if (isCurrentOwnerLocked(owner) && _activeOperation != null && _activeOperation.CallsignSegmentIndex >= 0 && _activeOperation.NextSegmentIndex <= _activeOperation.CallsignSegmentIndex)
				{
					string text = parseCallsignBase(decodeTciText(callsign), out var repeatCount);
					if (!string.IsNullOrWhiteSpace(text))
					{
						_activeOperation.CallsignBase = text;
						_activeOperation.Segments[_activeOperation.CallsignSegmentIndex].Text = buildRepeatedCallsign(text, repeatCount);
					}
				}
			}
		}

		public void OnRemoteCharacterStarted(int remainingRemoteCharacters, int pendingElements)
		{
			bool flag = false;
			int rx = 0;
			lock (_lockObj)
			{
				if (_activeOperation == null || !isTerminalEnabledLocked(_activeOperation.Rx) || _activeOperation.EmptyNotified || _pendingOperations.Count > 0 || _activeOperation.NextSegmentIndex < _activeOperation.Segments.Count || remainingRemoteCharacters > 0 || pendingElements <= 0)
				{
					return;
				}
				_activeOperation.EmptyNotified = true;
				rx = _activeOperation.Rx;
				flag = true;
			}
			if (flag)
			{
				_server.OnCwMacrosEmpty(rx);
			}
		}

		public void Stop(TCPIPtciSocketListener owner)
		{
			int num = -1;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			lock (_lockObj)
			{
				if (!isCurrentOwnerLocked(owner))
				{
					return;
				}
				cancelKeyerReleaseScheduleLocked();
				_pendingOperations.Clear();
				if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
				{
					num = _activeOperation.BaseSpeedWpm;
				}
				_activeOperation = null;
				flag3 = _keyerPressed || _keyerReleasePending;
				_keyerPressed = false;
				_keyerReleasePending = false;
				_keyerPressedAtTicks = -1L;
				_keyerReleaseAtTicks = -1L;
				_keyerRx = -1;
				flag = captureDirectKeyerMoxReleaseLocked();
				flag2 = true;
				_releaseTerminalTciPttWhenIdle = false;
				releaseOwnershipIfIdleLocked();
			}
			if (num > 0)
			{
				SetMacroSpeed(num);
			}
			InvokeOnConsole(delegate(Console c)
			{
				c.CWXForm.AbortSending();
			});
			if (flag3)
			{
				setDirectKeyerState(pressed: false);
			}
			if (flag)
			{
				releaseDirectKeyerMox();
			}
			if (flag2)
			{
				lock (_lockObj)
				{
					releaseTerminalTciPttIfOwnedLocked();
				}
			}
		}

		public void HandleTciPttReleased(TCPIPtciSocketListener owner)
		{
			lock (_lockObj)
			{
				if (isCurrentOwnerLocked(owner) && _activeOperation == null && _pendingOperations.Count <= 0 && (_keyerPressed || _keyerReleasePending))
				{
					releaseKeyerLocked();
				}
			}
		}

		public void DisconnectClient(TCPIPtciSocketListener owner)
		{
			if (owner == null)
			{
				return;
			}
			lock (_lockObj)
			{
				if (!isCurrentOwnerLocked(owner))
				{
					return;
				}
				for (int i = 0; i < _terminalEnabledByRx.Length; i++)
				{
					_terminalEnabledByRx[i] = false;
				}
				_releaseTerminalTciPttWhenIdle = false;
			}
			Stop(owner);
		}

		private static int clampMacroSpeed(int speed)
		{
			return Math.Max(1, Math.Min(99, speed));
		}

		private static string decodeTciText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			return text.Replace('^', ':').Replace('~', ',').Replace('*', ';');
		}

		private static string normalizeMessageField(string text)
		{
			text = decodeTciText(text);
			if (!(text == "_"))
			{
				return text;
			}
			return string.Empty;
		}

		private static string translateAbbreviationToken(string token)
		{
			string text = token;
			if (text == null)
			{
				text = string.Empty;
			}
			return text.Trim().ToUpperInvariant() switch
			{
				"SK" => "*", 
				"AR" => "+", 
				"KN" => "(", 
				"SN" => "!", 
				"BT" => "=", 
				"BK" => "\\", 
				"AS" => "%", 
				_ => text, 
			};
		}

		private static string buildRepeatedCallsign(string callsign, int repeatCount)
		{
			callsign = ((callsign != null) ? callsign : string.Empty).Trim();
			if (repeatCount < 2)
			{
				return callsign;
			}
			return string.Join(" ", Enumerable.Repeat(callsign, repeatCount));
		}

		private static string parseCallsignBase(string callsign, out int repeatCount)
		{
			repeatCount = 1;
			callsign = ((callsign != null) ? callsign : string.Empty).Trim();
			int num = callsign.LastIndexOf('$');
			if (num > 0 && num < callsign.Length - 1 && int.TryParse(callsign.Substring(num + 1), out var result))
			{
				repeatCount = Math.Max(1, result);
				callsign = callsign.Substring(0, num).Trim();
			}
			return callsign;
		}

		private static CWTextParseResult parseMacroText(string text, int startingSpeed)
		{
			CWTextParseResult result = new CWTextParseResult
			{
				FinalSpeedWpm = clampMacroSpeed(startingSpeed)
			};
			if (string.IsNullOrEmpty(text))
			{
				result.Segments.Add(new CWTxSegment
				{
					Text = " ",
					SpeedWpm = result.FinalSpeedWpm
				});
				return result;
			}
			StringBuilder current = new StringBuilder();
			Action action = delegate
			{
				if (current.Length >= 1)
				{
					result.Segments.Add(new CWTxSegment
					{
						Text = current.ToString(),
						SpeedWpm = result.FinalSpeedWpm
					});
					current.Clear();
				}
			};
			for (int num = 0; num < text.Length; num++)
			{
				char c = text[num];
				if (c == '|' && num + 1 < text.Length)
				{
					int num2 = text.IndexOf('|', num + 1);
					if (num2 > num + 1)
					{
						current.Append(translateAbbreviationToken(text.Substring(num + 1, num2 - num - 1)));
						num = num2;
						continue;
					}
				}
				if (c == '>' || c == '<')
				{
					action();
					result.UsedInlineSpeedChanges = true;
					result.FinalSpeedWpm = clampMacroSpeed(result.FinalSpeedWpm + ((c == '>') ? 5 : (-5)));
				}
				else
				{
					current.Append(c);
				}
			}
			action();
			if (result.Segments.Count < 1)
			{
				result.Segments.Add(new CWTxSegment
				{
					Text = " ",
					SpeedWpm = result.FinalSpeedWpm
				});
			}
			return result;
		}

		private CWTxOperation buildMacroOperation(int rx, string text)
		{
			int macroSpeed = GetMacroSpeed();
			CWTextParseResult cWTextParseResult = parseMacroText(decodeTciText(text), macroSpeed);
			CWTxOperation cWTxOperation = new CWTxOperation();
			cWTxOperation.Rx = rx;
			cWTxOperation.BaseSpeedWpm = macroSpeed;
			cWTxOperation.RestoreBaseSpeed = cWTextParseResult.UsedInlineSpeedChanges;
			cWTxOperation.Segments.AddRange(cWTextParseResult.Segments);
			return cWTxOperation;
		}

		private CWTxOperation buildMessageOperation(int rx, string prefix, string callsign, string suffix)
		{
			int macroSpeed = GetMacroSpeed();
			prefix = normalizeMessageField(prefix);
			suffix = normalizeMessageField(suffix);
			callsign = normalizeMessageField(callsign);
			string text = parseCallsignBase(callsign, out var repeatCount);
			if (string.IsNullOrWhiteSpace(text))
			{
				text = "?";
			}
			CWTextParseResult cWTextParseResult = parseMacroText(prefix, macroSpeed);
			CWTextParseResult cWTextParseResult2 = parseMacroText(suffix, cWTextParseResult.FinalSpeedWpm);
			CWTxOperation cWTxOperation = new CWTxOperation
			{
				Rx = rx,
				BaseSpeedWpm = macroSpeed,
				RestoreBaseSpeed = (cWTextParseResult.UsedInlineSpeedChanges || cWTextParseResult2.UsedInlineSpeedChanges),
				CallsignBase = text
			};
			if (!string.IsNullOrEmpty(prefix))
			{
				cWTxOperation.Segments.AddRange(cWTextParseResult.Segments);
			}
			cWTxOperation.CallsignSegmentIndex = cWTxOperation.Segments.Count;
			cWTxOperation.Segments.Add(new CWTxSegment
			{
				Text = buildRepeatedCallsign(text, repeatCount),
				SpeedWpm = cWTextParseResult.FinalSpeedWpm
			});
			if (!string.IsNullOrEmpty(suffix))
			{
				cWTxOperation.Segments.AddRange(cWTextParseResult2.Segments);
			}
			return cWTxOperation;
		}

		private void PollCallback(object state)
		{
			lock (_lockObj)
			{
				if (_disposed)
				{
					return;
				}
				if (_activeOperation != null && (!selectCwTargetLocked(_activeOperation.Rx) || !isCWModeLocked()))
				{
					abortOperationsForNonCWLocked();
				}
				else if ((_keyerPressed || _keyerReleasePending) && (_keyerRx < 0 || !selectCwTargetLocked(_keyerRx) || !isCWModeLocked()))
				{
					abortOperationsForNonCWLocked();
				}
				else
				{
					if (tryReleaseDirectKeyerFromPollLocked())
					{
						return;
					}
					if (_activeOperation == null)
					{
						if (_releaseTerminalTciPttWhenIdle)
						{
							releaseTerminalTciPttIfOwnedLocked();
						}
						startNextOperationLocked();
						return;
					}
					int num = InvokeOnConsole((Console c) => c.CWXForm.PendingRemoteCharacters, 0);
					int num2 = InvokeOnConsole((Console c) => c.CWXForm.Characters2Send, 0);
					bool num3 = num <= 0 && num2 <= 0;
					if (isTerminalEnabledLocked(_activeOperation.Rx) && !_activeOperation.EmptyNotified && _pendingOperations.Count < 1 && _activeOperation.NextSegmentIndex >= _activeOperation.Segments.Count && num <= 0 && num2 > 0)
					{
						_activeOperation.EmptyNotified = true;
						_server.OnCwMacrosEmpty(_activeOperation.Rx);
					}
					if (num3)
					{
						if (_activeOperation.ActiveSegmentIndex == _activeOperation.CallsignSegmentIndex && !_activeOperation.CallsignNotified)
						{
							_activeOperation.CallsignNotified = true;
							_server.OnCwCallsignSent(_activeOperation.CallsignBase);
						}
						if (_activeOperation.NextSegmentIndex < _activeOperation.Segments.Count)
						{
							queueNextSegmentLocked();
						}
						else
						{
							completeActiveOperationLocked();
						}
					}
				}
			}
		}

		private void startNextOperationLocked()
		{
			if (_activeOperation != null || _pendingOperations.Count < 1 || _keyerPressed)
			{
				return;
			}
			CWTxOperation cWTxOperation = _pendingOperations.Peek();
			if (!selectCwTargetLocked(cWTxOperation.Rx) || !isCWModeLocked())
			{
				abortOperationsForNonCWLocked();
				return;
			}
			_activeOperation = _pendingOperations.Dequeue();
			if (_terminalTciPttAsserted && _terminalTciPttRx != _activeOperation.Rx && !isTerminalEnabledLocked(_activeOperation.Rx))
			{
				releaseTerminalTciPttIfOwnedLocked();
			}
			ensureTerminalTciPttLocked();
			queueNextSegmentLocked();
		}

		private void queueNextSegmentLocked()
		{
			if (_activeOperation == null || _activeOperation.NextSegmentIndex >= _activeOperation.Segments.Count)
			{
				return;
			}
			if (!selectCwTargetLocked(_activeOperation.Rx) || !isCWModeLocked())
			{
				abortOperationsForNonCWLocked();
				return;
			}
			CWTxSegment cWTxSegment = _activeOperation.Segments[_activeOperation.NextSegmentIndex];
			string text = (string.IsNullOrEmpty(cWTxSegment.Text) ? " " : cWTxSegment.Text);
			ensureTerminalTciPttLocked();
			SetMacroSpeedSilently(cWTxSegment.SpeedWpm);
			_activeOperation.ActiveSegmentIndex = _activeOperation.NextSegmentIndex;
			_activeOperation.NextSegmentIndex++;
			InvokeOnConsole(delegate(Console c)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(text);
				c.CWXForm.RemoteMessage(bytes);
			});
		}

		private void completeActiveOperationLocked()
		{
			CWTxOperation activeOperation = _activeOperation;
			_activeOperation = null;
			if (activeOperation != null && activeOperation.RestoreBaseSpeed)
			{
				SetMacroSpeedSilently(activeOperation.BaseSpeedWpm);
			}
			if (activeOperation == null || !isTerminalEnabledLocked(activeOperation.Rx) || _releaseTerminalTciPttWhenIdle)
			{
				releaseTerminalTciPttIfOwnedLocked();
			}
			startNextOperationLocked();
			releaseOwnershipIfIdleLocked();
		}

		private bool isCWModeLocked()
		{
			DSPMode dSPMode = InvokeOnConsole((Console c) => (!c.RX2Enabled || !c.VFOBTX) ? c.RX1DSPMode : c.RX2DSPMode, DSPMode.FIRST);
			if (dSPMode != DSPMode.CWL)
			{
				return dSPMode == DSPMode.CWU;
			}
			return true;
		}

		private void abortOperationsForNonCWLocked()
		{
			int num = -1;
			cancelKeyerReleaseScheduleLocked();
			_pendingOperations.Clear();
			if (_activeOperation != null && _activeOperation.RestoreBaseSpeed)
			{
				num = _activeOperation.BaseSpeedWpm;
			}
			_activeOperation = null;
			bool flag = _keyerPressed || _keyerReleasePending;
			_keyerPressed = false;
			_keyerReleasePending = false;
			_keyerPressedAtTicks = -1L;
			_keyerReleaseAtTicks = -1L;
			_keyerRx = -1;
			bool num2 = captureDirectKeyerMoxReleaseLocked();
			if (num > 0)
			{
				SetMacroSpeedSilently(num);
			}
			InvokeOnConsole(delegate(Console c)
			{
				c.CWXForm.AbortSending();
			});
			if (flag)
			{
				setDirectKeyerState(pressed: false);
			}
			if (num2)
			{
				releaseDirectKeyerMox();
			}
			releaseTerminalTciPttIfOwnedLocked();
			releaseOwnershipIfIdleLocked();
		}

		private bool isCurrentOwnerLocked(TCPIPtciSocketListener owner)
		{
			if (owner != null)
			{
				if (_currentOwner != null)
				{
					return _currentOwner == owner;
				}
				return false;
			}
			return true;
		}

		private bool tryAcquireOwnershipLocked(TCPIPtciSocketListener owner)
		{
			if (owner == null)
			{
				return true;
			}
			if (_currentOwner == null || _currentOwner == owner)
			{
				_currentOwner = owner;
				return true;
			}
			return false;
		}

		private void releaseOwnershipIfIdleLocked()
		{
			if (!isAnyTerminalEnabledLocked() && _activeOperation == null && _pendingOperations.Count < 1 && !_keyerPressed && !_keyerReleasePending)
			{
				_currentOwner = null;
			}
		}

		private void KeyerSchedulerThreadProc()
		{
			while (true)
			{
				_keyerScheduleEvent.WaitOne();
				while (true)
				{
					long keyerReleaseAtTicks;
					lock (_lockObj)
					{
						if (_disposed)
						{
							return;
						}
						if (!_keyerReleasePending || !_keyerPressed)
						{
							break;
						}
						keyerReleaseAtTicks = _keyerReleaseAtTicks;
						goto IL_004f;
					}
					IL_004f:
					if (!waitForScheduledKeyerRelease(keyerReleaseAtTicks))
					{
						continue;
					}
					lock (_lockObj)
					{
						if (_disposed)
						{
							return;
						}
						if (_keyerReleasePending && _keyerPressed)
						{
							if (_keyerReleaseAtTicks != keyerReleaseAtTicks)
							{
								continue;
							}
							releaseKeyerLocked();
						}
					}
					break;
				}
			}
		}

		private void scheduleKeyerReleaseLocked(int durationMs)
		{
			long num = _keyerPressedAtTicks + millisecondsToStopwatchTicks(Math.Max(0, durationMs));
			if (num - _keyerStopwatch.ElapsedTicks <= 0)
			{
				releaseKeyerLocked();
				return;
			}
			_keyerReleasePending = true;
			_keyerReleaseAtTicks = num;
			_keyerScheduleEvent.Set();
		}

		private bool tryReleaseDirectKeyerFromPollLocked()
		{
			if (!_keyerPressed)
			{
				return false;
			}
			long elapsedTicks = _keyerStopwatch.ElapsedTicks;
			if (_keyerReleasePending)
			{
				if (_keyerReleaseAtTicks >= 0 && elapsedTicks >= _keyerReleaseAtTicks)
				{
					releaseKeyerLocked();
					return true;
				}
				return false;
			}
			if (_keyerPressedAtTicks < 0)
			{
				return false;
			}
			long num = _keyerPressedAtTicks + millisecondsToStopwatchTicks(3000);
			if (elapsedTicks < num)
			{
				return false;
			}
			releaseKeyerLocked();
			return true;
		}

		private void releaseKeyerLocked()
		{
			cancelKeyerReleaseScheduleLocked();
			if (_keyerPressed || _keyerReleasePending)
			{
				_keyerPressed = false;
				_keyerReleasePending = false;
				_keyerPressedAtTicks = -1L;
				_keyerReleaseAtTicks = -1L;
				_keyerRx = -1;
				bool num = captureDirectKeyerMoxReleaseLocked();
				setDirectKeyerState(pressed: false);
				if (num)
				{
					releaseDirectKeyerMox();
				}
				releaseOwnershipIfIdleLocked();
			}
		}

		private void cancelKeyerReleaseScheduleLocked()
		{
			_keyerReleasePending = false;
			_keyerReleaseAtTicks = -1L;
			_keyerScheduleEvent.Set();
		}

		private bool isCwTargetAvailableLocked(int rx)
		{
			if (rx != 1)
			{
				return true;
			}
			return InvokeOnConsole((Console c) => c.RX2Enabled, defaultValue: false);
		}

		private bool selectCwTargetLocked(int rx)
		{
			return InvokeOnConsole(delegate(Console c)
			{
				if (rx == 1)
				{
					if (!c.RX2Enabled)
					{
						return false;
					}
					if (!c.VFOBTX)
					{
						c.VFOBTX = true;
					}
					return true;
				}
				if (c.RX2Enabled && c.VFOBTX)
				{
					c.VFOATX = true;
				}
				return true;
			}, defaultValue: false);
		}

		private void ensureTerminalTciPttLocked()
		{
			if (_activeOperation == null || !isTerminalEnabledLocked(_activeOperation.Rx))
			{
				return;
			}
			if (_terminalTciPttAsserted)
			{
				_terminalTciPttRx = _activeOperation.Rx;
			}
			else if (!InvokeOnConsole((Console c) => c.TCIPTT, defaultValue: false))
			{
				InvokeOnConsole(delegate(Console c)
				{
					c.TCIPTT = true;
				});
				_terminalTciPttAsserted = true;
				_terminalTciPttRx = _activeOperation.Rx;
			}
		}

		private void releaseTerminalTciPttIfOwnedLocked()
		{
			if (_terminalTciPttAsserted)
			{
				InvokeOnConsole(delegate(Console c)
				{
					c.TCIPTT = false;
				});
			}
			_terminalTciPttAsserted = false;
			_terminalTciPttRx = -1;
			_releaseTerminalTciPttWhenIdle = false;
		}

		private bool isTerminalEnabledLocked(int rx)
		{
			if (rx >= 0 && rx < _terminalEnabledByRx.Length)
			{
				return _terminalEnabledByRx[rx];
			}
			return false;
		}

		private bool isAnyTerminalEnabledLocked()
		{
			for (int i = 0; i < _terminalEnabledByRx.Length; i++)
			{
				if (_terminalEnabledByRx[i])
				{
					return true;
				}
			}
			return false;
		}

		private bool beginDirectKeyerLocked()
		{
			if (!InvokeOnConsole((Console c) => !c.DisablePTT, defaultValue: false))
			{
				return false;
			}
			if (!ensureDirectKeyerMoxLocked())
			{
				return false;
			}
			if (setDirectKeyerState(pressed: true))
			{
				return true;
			}
			if (captureDirectKeyerMoxReleaseLocked())
			{
				releaseDirectKeyerMox();
			}
			return false;
		}

		private bool ensureDirectKeyerMoxLocked()
		{
			if (InvokeOnConsole((Console c) => c.CurrentBreakInMode, BreakIn.Manual) != BreakIn.Semi)
			{
				_keyerAssertedMox = false;
				return true;
			}
			if (InvokeOnConsole((Console c) => c.MOX, defaultValue: false))
			{
				_keyerAssertedMox = false;
				return true;
			}
			bool flag = InvokeOnConsole((Console c) => c.TCIPTT, defaultValue: false);
			bool flag2 = InvokeOnConsole(delegate(Console c)
			{
				if (!c.MOX)
				{
					c.MOX = true;
				}
				return c.MOX;
			}, defaultValue: false);
			_keyerAssertedMox = flag2 && !flag;
			return flag2;
		}

		private bool captureDirectKeyerMoxReleaseLocked()
		{
			bool result = _keyerAssertedMox && !InvokeOnConsole((Console c) => c.TCIPTT, defaultValue: false);
			_keyerAssertedMox = false;
			return result;
		}

		private void releaseDirectKeyerMox()
		{
			InvokeOnConsole(delegate(Console c)
			{
				if (c.MOX)
				{
					c.MOX = false;
				}
				return 0;
			}, 0);
		}

		private bool waitForScheduledKeyerRelease(long releaseAtTicks)
		{
			while (true)
			{
				long num = releaseAtTicks - _keyerStopwatch.ElapsedTicks;
				if (num <= 0)
				{
					return true;
				}
				double num2 = stopwatchTicksToMilliseconds(num);
				if (num2 > 2.0)
				{
					int millisecondsTimeout = Math.Max(1, (int)Math.Floor(num2) - 1);
					if (_keyerScheduleEvent.WaitOne(millisecondsTimeout))
					{
						break;
					}
				}
				else
				{
					Thread.SpinWait(128);
				}
			}
			return false;
		}

		private static long millisecondsToStopwatchTicks(int durationMs)
		{
			if (durationMs <= 0)
			{
				return 0L;
			}
			return (long)Math.Round((double)durationMs / 1000.0 * (double)Stopwatch.Frequency, MidpointRounding.AwayFromZero);
		}

		private static double stopwatchTicksToMilliseconds(long ticks)
		{
			return (double)ticks * 1000.0 / (double)Stopwatch.Frequency;
		}

		private static bool setDirectKeyerState(bool pressed)
		{
			try
			{
				NetworkIO.SetCWX(pressed ? 1 : 0);
				NetworkIO.SendHighPriority(1);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private T InvokeOnConsole<T>(Func<Console, T> action, T defaultValue)
		{
			Console console = _server._console;
			if (console == null || console.IsDisposed)
			{
				return defaultValue;
			}
			try
			{
				if (console.InvokeRequired)
				{
					return (T)console.Invoke(action, console);
				}
				return action(console);
			}
			catch
			{
				return defaultValue;
			}
		}

		private void InvokeOnConsole(Action<Console> action)
		{
			Console console = _server._console;
			if (console == null || console.IsDisposed)
			{
				return;
			}
			try
			{
				if (console.InvokeRequired)
				{
					console.Invoke(action, console);
				}
				else
				{
					action(console);
				}
			}
			catch
			{
			}
		}
	}

	public ClientConnected ClientConnectedHandlers;

	public ClientDisconnected ClientDisconnectedHandlers;

	public ClientError ClientErrorHandlers;

	public ServerError ServerErrorHandlers;

	private Console _console;

	public static IPAddress DEFAULT_SERVER = IPAddress.Parse("127.0.0.1");

	public static int DEFAULT_PORT = 31001;

	public static IPEndPoint DEFAULT_IP_END_POINT = new IPEndPoint(DEFAULT_SERVER, DEFAULT_PORT);

	private TcpListener m_server;

	private bool m_stopServer;

	private bool m_stopPurging;

	private Thread m_serverThread;

	private Thread m_purgingThread;

	private List<TCPIPtciSocketListener> m_socketListenersList;

	private TCPIPtciSocketListener m_activeTxAudioListener;

	private object m_objLocker = new object();

	private bool m_bSleepingInPurge;

	private bool m_bDelegatesAdded;

	private int m_nRateLimit;

	private bool m_bEmulateSunSDR2Pro;

	private bool m_bEmulateExpertSDR3Protocol = true;

	private bool m_bIQSwap = true;

	private bool m_bAlwaysStreamIQ;

	private TCITxStereoInputMode m_txStereoInputMode = TCITxStereoInputMode.Both;

	private TCICWController m_cwController;

	private int m_cwInternalMacroSpeedUpdates;

	private frmLog _log;

	private bool m_bCopyRX2VFObToVFOa;

	private bool _replace_if_copy_RX2VFObToVFOa;

	private bool m_bCWLUbecomesCW;

	private bool m_bCWbecomesCWUabove10mhz;

	private TCICWSpotForce _spot_force = TCICWSpotForce.DEFAULT;

	private bool m_bUseRX1VFOaForRX2VFOa;

	private bool m_bSendInitialStateOnConnect = true;

	private string m_sLastError = "";

	private Console console
	{
		get
		{
			if (_console == null)
			{
				return null;
			}
			if (_console.InvokeRequired)
			{
				return (Console)_console.Invoke((Func<Console>)(() => _console));
			}
			return _console;
		}
	}

	public frmLog LogForm => _log;

	public bool CopyRX2VFObToVFOa
	{
		get
		{
			return m_bCopyRX2VFObToVFOa;
		}
		set
		{
			m_bCopyRX2VFObToVFOa = value;
		}
	}

	public bool ReplaceRX2VFObIfCopyBtoA
	{
		get
		{
			return _replace_if_copy_RX2VFObToVFOa;
		}
		set
		{
			_replace_if_copy_RX2VFObToVFOa = value;
		}
	}

	public bool CWLUbecomesCW
	{
		get
		{
			return m_bCWLUbecomesCW;
		}
		set
		{
			m_bCWLUbecomesCW = value;
		}
	}

	public bool CWbecomesCWUabove10mhz
	{
		get
		{
			return m_bCWbecomesCWUabove10mhz;
		}
		set
		{
			m_bCWbecomesCWUabove10mhz = value;
		}
	}

	public TCICWSpotForce CWSpotForce
	{
		get
		{
			return _spot_force;
		}
		set
		{
			_spot_force = value;
		}
	}

	public bool UseRX1VFOaForRX2VFOa
	{
		get
		{
			return m_bUseRX1VFOaForRX2VFOa;
		}
		set
		{
			m_bUseRX1VFOaForRX2VFOa = value;
		}
	}

	public bool SendInitialFrequencyStateOnConnect
	{
		get
		{
			return m_bSendInitialStateOnConnect;
		}
		set
		{
			m_bSendInitialStateOnConnect = value;
		}
	}

	public bool EmulateSunSDR2Pro
	{
		get
		{
			return m_bEmulateSunSDR2Pro;
		}
		set
		{
			m_bEmulateSunSDR2Pro = value;
		}
	}

	public bool EmulateExpertSDR3Protocol
	{
		get
		{
			return m_bEmulateExpertSDR3Protocol;
		}
		set
		{
			m_bEmulateExpertSDR3Protocol = value;
		}
	}

	public bool IQSwap
	{
		get
		{
			return m_bIQSwap;
		}
		set
		{
			m_bIQSwap = value;
		}
	}

	public bool AlwaysStreamIQ
	{
		get
		{
			return m_bAlwaysStreamIQ;
		}
		set
		{
			m_bAlwaysStreamIQ = value;
			RefreshStreamRunState();
		}
	}

	public TCITxStereoInputMode TXStereoInputMode
	{
		get
		{
			return m_txStereoInputMode;
		}
		set
		{
			m_txStereoInputMode = value;
		}
	}

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
			int num = 0;
			lock (m_objLocker)
			{
				if (m_server == null || m_socketListenersList == null)
				{
					return num;
				}
				foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
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

	public TCPIPtciServer()
	{
		Init(DEFAULT_IP_END_POINT);
	}

	public TCPIPtciServer(IPAddress serverIP)
	{
		Init(new IPEndPoint(serverIP, DEFAULT_PORT));
	}

	public TCPIPtciServer(int port)
	{
		Init(new IPEndPoint(DEFAULT_SERVER, port));
	}

	public TCPIPtciServer(IPAddress serverIP, int port)
	{
		Init(new IPEndPoint(serverIP, port));
	}

	public TCPIPtciServer(IPEndPoint ipNport)
	{
		Init(ipNport);
	}

	~TCPIPtciServer()
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
		catch
		{
			m_server = null;
		}
	}

	public void StartServer(Console c, int rateLimit = 0)
	{
		if (m_server != null)
		{
			m_nRateLimit = rateLimit;
			if (c != null && !c.IsSetupFormNull)
			{
				m_bSendInitialStateOnConnect = c.SetupForm.TCIsendInitialStateOnConnect;
				m_bCopyRX2VFObToVFOa = c.SetupForm.TCIcopyRX2VFObToVFOa;
				_replace_if_copy_RX2VFObToVFOa = c.SetupForm.TCIreplaceRX2VFObToVFOa;
				m_bUseRX1VFOaForRX2VFOa = c.SetupForm.TCIuseRX1vfoaForRX2vfoa;
				m_bCWLUbecomesCW = c.SetupForm.TCICWLUbecomesCW;
				m_bCWbecomesCWUabove10mhz = c.SetupForm.TCICWbecomesCWUabove10mhz;
				m_bEmulateSunSDR2Pro = c.SetupForm.EmulateSunSDR2Pro;
				m_bEmulateExpertSDR3Protocol = c.SetupForm.EmulateExpertSDR3Protocol;
				_spot_force = c.SetupForm.CWSpotForce;
				m_bIQSwap = c.SetupForm.TCISwapIQ;
				m_bAlwaysStreamIQ = c.SetupForm.TCIAlwaysStreamIQ;
				m_txStereoInputMode = c.SetupForm.TCITXInputChannel;
			}
			else
			{
				m_bSendInitialStateOnConnect = true;
				m_bCopyRX2VFObToVFOa = false;
				_replace_if_copy_RX2VFObToVFOa = false;
				m_bUseRX1VFOaForRX2VFOa = false;
				m_bCWLUbecomesCW = false;
				m_bCWbecomesCWUabove10mhz = false;
				m_bEmulateSunSDR2Pro = false;
				m_bEmulateExpertSDR3Protocol = false;
				_spot_force = TCICWSpotForce.DEFAULT;
				m_bIQSwap = true;
				m_bAlwaysStreamIQ = false;
				m_txStereoInputMode = TCITxStereoInputMode.Both;
			}
			_console = c;
			if (m_cwController != null)
			{
				m_cwController.Dispose();
				m_cwController = null;
			}
			if (_console != null)
			{
				m_cwController = new TCICWController(this);
			}
			m_socketListenersList = new List<TCPIPtciSocketListener>();
			cmaster.SetRXTCIRun(0);
			if (console != null && !m_bDelegatesAdded)
			{
				Console obj = console;
				obj.VFOAFrequencyChangeHandlers = (Console.VFOAFrequencyChanged)Delegate.Combine(obj.VFOAFrequencyChangeHandlers, new Console.VFOAFrequencyChanged(OnVFOAFrequencyChangeHandler));
				Console obj2 = console;
				obj2.VFOBFrequencyChangeHandlers = (Console.VFOBFrequencyChanged)Delegate.Combine(obj2.VFOBFrequencyChangeHandlers, new Console.VFOBFrequencyChanged(OnVFOBFrequencyChangeHandler));
				Console obj3 = console;
				obj3.MoxChangeHandlers = (Console.MoxChanged)Delegate.Combine(obj3.MoxChangeHandlers, new Console.MoxChanged(OnMoxChangeHandler));
				Console obj4 = console;
				obj4.MoxPreChangeHandlers = (Console.MoxPreChanged)Delegate.Combine(obj4.MoxPreChangeHandlers, new Console.MoxPreChanged(OnMoxPreChangeHandler));
				Console obj5 = console;
				obj5.ModeChangeHandlers = (Console.ModeChanged)Delegate.Combine(obj5.ModeChangeHandlers, new Console.ModeChanged(OnModeChangeHandler));
				Console obj6 = console;
				obj6.BandChangeHandlers = (Console.BandChanged)Delegate.Combine(obj6.BandChangeHandlers, new Console.BandChanged(OnBandChangeHandler));
				Console obj7 = console;
				obj7.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Combine(obj7.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequencyChanged));
				Console obj8 = console;
				obj8.FilterChangedHandlers = (Console.FilterChanged)Delegate.Combine(obj8.FilterChangedHandlers, new Console.FilterChanged(OnFilterChanged));
				Console obj9 = console;
				obj9.FilterEdgesChangedHandlers = (Console.FilterEdgesChanged)Delegate.Combine(obj9.FilterEdgesChangedHandlers, new Console.FilterEdgesChanged(OnFilterEdgesChanged));
				Console obj10 = console;
				obj10.TXFiltersChangedHandlers = (Console.TXFiltersChanged)Delegate.Combine(obj10.TXFiltersChangedHandlers, new Console.TXFiltersChanged(OnTXFiltersChanged));
				Console obj11 = console;
				obj11.PowerChangeHanders = (Console.PowerChanged)Delegate.Combine(obj11.PowerChangeHanders, new Console.PowerChanged(OnPowerChangeHander));
				Console obj12 = console;
				obj12.SplitChangedHandlers = (Console.SplitChanged)Delegate.Combine(obj12.SplitChangedHandlers, new Console.SplitChanged(OnSplitChanged));
				Console obj13 = console;
				obj13.TuneChangedHandlers = (Console.TuneChanged)Delegate.Combine(obj13.TuneChangedHandlers, new Console.TuneChanged(OnTuneChanged));
				Console obj14 = console;
				obj14.DrivePowerChangedHandlers = (Console.DrivePowerChanged)Delegate.Combine(obj14.DrivePowerChangedHandlers, new Console.DrivePowerChanged(OnDrivePowerChanged));
				Console obj15 = console;
				obj15.HWSampleRateChangedHandlers = (Console.HWSampleRateChanged)Delegate.Combine(obj15.HWSampleRateChangedHandlers, new Console.HWSampleRateChanged(OnHWSampleRateChanged));
				Console obj16 = console;
				obj16.ThetisFocusChangedHandlers = (Console.ThetisFocusChanged)Delegate.Combine(obj16.ThetisFocusChangedHandlers, new Console.ThetisFocusChanged(OnThetisFocusChanged));
				Console obj17 = console;
				obj17.RX2EnabledChangedHandlers = (Console.RX2EnabledChanged)Delegate.Combine(obj17.RX2EnabledChangedHandlers, new Console.RX2EnabledChanged(OnRX2EnabledChanged));
				Console obj18 = console;
				obj18.SpotClickedHandlers = (Console.SpotClicked)Delegate.Combine(obj18.SpotClickedHandlers, new Console.SpotClicked(OnSpotClicked));
				Console obj19 = console;
				obj19.MuteChangedHandlers = (Console.MuteChanged)Delegate.Combine(obj19.MuteChangedHandlers, new Console.MuteChanged(OnMuteChanged));
				Console obj20 = console;
				obj20.MONChangedHandlers = (Console.MONChanged)Delegate.Combine(obj20.MONChangedHandlers, new Console.MONChanged(OnMONChanged));
				Console obj21 = console;
				obj21.MONVolumeChangedHandlers = (Console.MONVolumeChanged)Delegate.Combine(obj21.MONVolumeChangedHandlers, new Console.MONVolumeChanged(OnMONVolumeChanged));
				Console obj22 = console;
				obj22.VolumeChangedHandlers = (Console.VolumeChanged)Delegate.Combine(obj22.VolumeChangedHandlers, new Console.VolumeChanged(OnVolumeChanged));
				Console obj23 = console;
				obj23.BalanceChangedHandlers = (Console.BalanceChanged)Delegate.Combine(obj23.BalanceChangedHandlers, new Console.BalanceChanged(OnBalanceChanged));
				Console obj24 = console;
				obj24.StepAttEnabledChangedHandlers = (Console.StepAttEnabledChanged)Delegate.Combine(obj24.StepAttEnabledChangedHandlers, new Console.StepAttEnabledChanged(OnStepAttEnabledChanged));
				Console obj25 = console;
				obj25.AttenuatorDataChangedHandlers = (Console.AttenuatorDataChanged)Delegate.Combine(obj25.AttenuatorDataChangedHandlers, new Console.AttenuatorDataChanged(OnAttenuatorDataChanged));
				Console obj26 = console;
				obj26.PreampModeChangedHandlers = (Console.PreampModeChanged)Delegate.Combine(obj26.PreampModeChangedHandlers, new Console.PreampModeChanged(OnPreampModeChanged));
				Console obj27 = console;
				obj27.FMDeviationChangedHandlers = (Console.FMDeviationChanged)Delegate.Combine(obj27.FMDeviationChangedHandlers, new Console.FMDeviationChanged(OnFMDeviationChanged));
				Console obj28 = console;
				obj28.AGCGainChangedHandlers = (Console.AGCGainChanged)Delegate.Combine(obj28.AGCGainChangedHandlers, new Console.AGCGainChanged(OnAGCGainChanged));
				Console obj29 = console;
				obj29.RITChangedHandlers = (Console.RITChanged)Delegate.Combine(obj29.RITChangedHandlers, new Console.RITChanged(OnRITChanged));
				Console obj30 = console;
				obj30.XITChangedHandlers = (Console.XITChanged)Delegate.Combine(obj30.XITChangedHandlers, new Console.XITChanged(OnXITChanged));
				Console obj31 = console;
				obj31.RITValueChangedHandlers = (Console.RITValueChanged)Delegate.Combine(obj31.RITValueChangedHandlers, new Console.RITValueChanged(OnRITValueChanged));
				Console obj32 = console;
				obj32.XITValueChangedHandlers = (Console.XITValueChanged)Delegate.Combine(obj32.XITValueChangedHandlers, new Console.XITValueChanged(OnXITValueChanged));
				Console obj33 = console;
				obj33.TXFrequncyChangedHandlers = (Console.TXFrequncyChanged)Delegate.Combine(obj33.TXFrequncyChangedHandlers, new Console.TXFrequncyChanged(OnTXFrequencyChanged));
				Console obj34 = console;
				obj34.MeterReadingsChangedHandlers = (Console.MeterReadings)Delegate.Combine(obj34.MeterReadingsChangedHandlers, new Console.MeterReadings(OnMeterReadingsChanged));
				Console obj35 = console;
				obj35.NRChangedHandlers = (Console.NRChanged)Delegate.Combine(obj35.NRChangedHandlers, new Console.NRChanged(OnNrChanged));
				Console obj36 = console;
				obj36.NBChangedHandlers = (Console.NBChanged)Delegate.Combine(obj36.NBChangedHandlers, new Console.NBChanged(OnNbChanged));
				Console obj37 = console;
				obj37.ANFChangedHandlers = (Console.ANFChanged)Delegate.Combine(obj37.ANFChangedHandlers, new Console.ANFChanged(OnAnfChanged));
				Console obj38 = console;
				obj38.BINChangedHandlers = (Console.BINChanged)Delegate.Combine(obj38.BINChangedHandlers, new Console.BINChanged(OnBinChanged));
				Console obj39 = console;
				obj39.AGCModeChangedHandlers = (Console.AGCModeChanged)Delegate.Combine(obj39.AGCModeChangedHandlers, new Console.AGCModeChanged(OnAGCModeChanged));
				Console obj40 = console;
				obj40.AGCAutoModeChangedHandlers = (Console.AGCAutoModeChanged)Delegate.Combine(obj40.AGCAutoModeChangedHandlers, new Console.AGCAutoModeChanged(OnAGCAutoModeChanged));
				Console obj41 = console;
				obj41.VFOSyncChangedHandlers = (Console.VFOSyncChanged)Delegate.Combine(obj41.VFOSyncChangedHandlers, new Console.VFOSyncChanged(OnVFOSyncChanged));
				Console obj42 = console;
				obj42.VfoALockChangedHandlers = (Console.VfoALockChanged)Delegate.Combine(obj42.VfoALockChangedHandlers, new Console.VfoALockChanged(OnVfoALockChanged));
				Console obj43 = console;
				obj43.VfoBLockChangedHandlers = (Console.VfoBLockChanged)Delegate.Combine(obj43.VfoBLockChangedHandlers, new Console.VfoBLockChanged(OnVfoBLockChanged));
				Console obj44 = console;
				obj44.SQLChangedHandlers = (Console.SQLChanged)Delegate.Combine(obj44.SQLChangedHandlers, new Console.SQLChanged(OnSqlChanged));
				Console obj45 = console;
				obj45.SQLLevelChangedHandlers = (Console.SQLLevelChanged)Delegate.Combine(obj45.SQLLevelChangedHandlers, new Console.SQLLevelChanged(OnSqlLevelChanged));
				Console obj46 = console;
				obj46.APFChangedHandlers = (Console.APFChanged)Delegate.Combine(obj46.APFChangedHandlers, new Console.APFChanged(OnApfChanged));
				Console obj47 = console;
				obj47.TNFChangedHandlers = (Console.TNFChanged)Delegate.Combine(obj47.TNFChangedHandlers, new Console.TNFChanged(OnTnfChanged));
				Console obj48 = console;
				obj48.DIGLOffsetChangedHandlers = (Console.DIGLOffsetChanged)Delegate.Combine(obj48.DIGLOffsetChangedHandlers, new Console.DIGLOffsetChanged(OnDiglOffsetChanged));
				Console obj49 = console;
				obj49.DIGUOffsetChangedHandlers = (Console.DIGUOffsetChanged)Delegate.Combine(obj49.DIGUOffsetChangedHandlers, new Console.DIGUOffsetChanged(OnDiguOffsetChanged));
				Console obj50 = console;
				obj50.CWXSpeedChangedHandlers = (Console.CWXSpeedChanged)Delegate.Combine(obj50.CWXSpeedChangedHandlers, new Console.CWXSpeedChanged(OnCwMacrosSpeedChanged));
				Console obj51 = console;
				obj51.CWXDelayChangedHandlers = (Console.CWXDelayChanged)Delegate.Combine(obj51.CWXDelayChangedHandlers, new Console.CWXDelayChanged(OnCwMacrosDelayChanged));
				Console obj52 = console;
				obj52.CWXRemoteCharacterStartedHandlers = (Console.CWXRemoteCharacterStarted)Delegate.Combine(obj52.CWXRemoteCharacterStartedHandlers, new Console.CWXRemoteCharacterStarted(OnCwRemoteCharacterStarted));
				Console obj53 = console;
				obj53.CWKeyerSpeedChangedHandlers = (Console.CWKeyerSpeedChanged)Delegate.Combine(obj53.CWKeyerSpeedChangedHandlers, new Console.CWKeyerSpeedChanged(OnCwKeyerSpeedChanged));
				Console obj54 = console;
				obj54.RXGainChangedHandlers = (Console.RXGainChanged)Delegate.Combine(obj54.RXGainChangedHandlers, new Console.RXGainChanged(OnRxAfGainChanged));
				Console obj55 = console;
				obj55.CTUNChangedHandlers = (Console.CTUNChanged)Delegate.Combine(obj55.CTUNChangedHandlers, new Console.CTUNChanged(OnCTUNChanged));
				Console obj56 = console;
				obj56.TXProfileChangedHandlers = (Console.TXProfileChanged)Delegate.Combine(obj56.TXProfileChangedHandlers, new Console.TXProfileChanged(OnTXProfileChanged));
				Console obj57 = console;
				obj57.TXProfilesChangedHandlers = (Console.TXProfilesChanged)Delegate.Combine(obj57.TXProfilesChangedHandlers, new Console.TXProfilesChanged(OnTXProfilesChanged));
				Console obj58 = console;
				obj58.MeterCalOffsetChangedHandlers = (Console.MeterCalOffsetChanged)Delegate.Combine(obj58.MeterCalOffsetChangedHandlers, new Console.MeterCalOffsetChanged(OnCalibrationChanged));
				Console obj59 = console;
				obj59.DisplayOffsetChangedHandlers = (Console.DisplayOffsetChanged)Delegate.Combine(obj59.DisplayOffsetChangedHandlers, new Console.DisplayOffsetChanged(OnCalibrationChanged));
				Console obj60 = console;
				obj60.XvtrGainOffsetChangedHandlers = (Console.XvtrGainOffsetChanged)Delegate.Combine(obj60.XvtrGainOffsetChangedHandlers, new Console.XvtrGainOffsetChanged(OnCalibrationChanged));
				Console obj61 = console;
				obj61.Rx6mOffsetChangedHandlers = (Console.Rx6mOffsetChanged)Delegate.Combine(obj61.Rx6mOffsetChangedHandlers, new Console.Rx6mOffsetChanged(OnCalibrationChanged));
				m_bDelegatesAdded = true;
			}
			try
			{
				m_server.Start();
				m_serverThread = new Thread(ServerThreadStart);
				m_serverThread.Priority = ThreadPriority.BelowNormal;
				m_serverThread.Name = "TCI server Thread";
				m_serverThread.Start();
				m_purgingThread = new Thread(PurgingThreadStart);
				m_purgingThread.Priority = ThreadPriority.Lowest;
				m_purgingThread.Name = "TCI purging Thread";
				m_purgingThread.Start();
			}
			catch (SocketException ex)
			{
				m_sLastError = ex.Message;
				StopServer();
				ServerErrorHandlers?.Invoke(ex);
			}
			catch
			{
				StopServer();
			}
		}
	}

	public void StopServer()
	{
		if (m_server == null)
		{
			return;
		}
		if (m_bDelegatesAdded)
		{
			Console obj = console;
			obj.VFOAFrequencyChangeHandlers = (Console.VFOAFrequencyChanged)Delegate.Remove(obj.VFOAFrequencyChangeHandlers, new Console.VFOAFrequencyChanged(OnVFOAFrequencyChangeHandler));
			Console obj2 = console;
			obj2.VFOBFrequencyChangeHandlers = (Console.VFOBFrequencyChanged)Delegate.Remove(obj2.VFOBFrequencyChangeHandlers, new Console.VFOBFrequencyChanged(OnVFOBFrequencyChangeHandler));
			Console obj3 = console;
			obj3.MoxChangeHandlers = (Console.MoxChanged)Delegate.Remove(obj3.MoxChangeHandlers, new Console.MoxChanged(OnMoxChangeHandler));
			Console obj4 = console;
			obj4.MoxPreChangeHandlers = (Console.MoxPreChanged)Delegate.Remove(obj4.MoxPreChangeHandlers, new Console.MoxPreChanged(OnMoxPreChangeHandler));
			Console obj5 = console;
			obj5.ModeChangeHandlers = (Console.ModeChanged)Delegate.Remove(obj5.ModeChangeHandlers, new Console.ModeChanged(OnModeChangeHandler));
			Console obj6 = console;
			obj6.BandChangeHandlers = (Console.BandChanged)Delegate.Remove(obj6.BandChangeHandlers, new Console.BandChanged(OnBandChangeHandler));
			Console obj7 = console;
			obj7.CentreFrequencyHandlers = (Console.CentreFrequencyChanged)Delegate.Remove(obj7.CentreFrequencyHandlers, new Console.CentreFrequencyChanged(OnCentreFrequencyChanged));
			Console obj8 = console;
			obj8.FilterChangedHandlers = (Console.FilterChanged)Delegate.Remove(obj8.FilterChangedHandlers, new Console.FilterChanged(OnFilterChanged));
			Console obj9 = console;
			obj9.FilterEdgesChangedHandlers = (Console.FilterEdgesChanged)Delegate.Remove(obj9.FilterEdgesChangedHandlers, new Console.FilterEdgesChanged(OnFilterEdgesChanged));
			Console obj10 = console;
			obj10.TXFiltersChangedHandlers = (Console.TXFiltersChanged)Delegate.Remove(obj10.TXFiltersChangedHandlers, new Console.TXFiltersChanged(OnTXFiltersChanged));
			Console obj11 = console;
			obj11.PowerChangeHanders = (Console.PowerChanged)Delegate.Remove(obj11.PowerChangeHanders, new Console.PowerChanged(OnPowerChangeHander));
			Console obj12 = console;
			obj12.SplitChangedHandlers = (Console.SplitChanged)Delegate.Remove(obj12.SplitChangedHandlers, new Console.SplitChanged(OnSplitChanged));
			Console obj13 = console;
			obj13.TuneChangedHandlers = (Console.TuneChanged)Delegate.Remove(obj13.TuneChangedHandlers, new Console.TuneChanged(OnTuneChanged));
			Console obj14 = console;
			obj14.DrivePowerChangedHandlers = (Console.DrivePowerChanged)Delegate.Remove(obj14.DrivePowerChangedHandlers, new Console.DrivePowerChanged(OnDrivePowerChanged));
			Console obj15 = console;
			obj15.HWSampleRateChangedHandlers = (Console.HWSampleRateChanged)Delegate.Remove(obj15.HWSampleRateChangedHandlers, new Console.HWSampleRateChanged(OnHWSampleRateChanged));
			Console obj16 = console;
			obj16.ThetisFocusChangedHandlers = (Console.ThetisFocusChanged)Delegate.Remove(obj16.ThetisFocusChangedHandlers, new Console.ThetisFocusChanged(OnThetisFocusChanged));
			Console obj17 = console;
			obj17.RX2EnabledChangedHandlers = (Console.RX2EnabledChanged)Delegate.Remove(obj17.RX2EnabledChangedHandlers, new Console.RX2EnabledChanged(OnRX2EnabledChanged));
			Console obj18 = console;
			obj18.SpotClickedHandlers = (Console.SpotClicked)Delegate.Remove(obj18.SpotClickedHandlers, new Console.SpotClicked(OnSpotClicked));
			Console obj19 = console;
			obj19.MuteChangedHandlers = (Console.MuteChanged)Delegate.Remove(obj19.MuteChangedHandlers, new Console.MuteChanged(OnMuteChanged));
			Console obj20 = console;
			obj20.MONChangedHandlers = (Console.MONChanged)Delegate.Remove(obj20.MONChangedHandlers, new Console.MONChanged(OnMONChanged));
			Console obj21 = console;
			obj21.MONVolumeChangedHandlers = (Console.MONVolumeChanged)Delegate.Remove(obj21.MONVolumeChangedHandlers, new Console.MONVolumeChanged(OnMONVolumeChanged));
			Console obj22 = console;
			obj22.VolumeChangedHandlers = (Console.VolumeChanged)Delegate.Remove(obj22.VolumeChangedHandlers, new Console.VolumeChanged(OnVolumeChanged));
			Console obj23 = console;
			obj23.BalanceChangedHandlers = (Console.BalanceChanged)Delegate.Remove(obj23.BalanceChangedHandlers, new Console.BalanceChanged(OnBalanceChanged));
			Console obj24 = console;
			obj24.StepAttEnabledChangedHandlers = (Console.StepAttEnabledChanged)Delegate.Remove(obj24.StepAttEnabledChangedHandlers, new Console.StepAttEnabledChanged(OnStepAttEnabledChanged));
			Console obj25 = console;
			obj25.AttenuatorDataChangedHandlers = (Console.AttenuatorDataChanged)Delegate.Remove(obj25.AttenuatorDataChangedHandlers, new Console.AttenuatorDataChanged(OnAttenuatorDataChanged));
			Console obj26 = console;
			obj26.PreampModeChangedHandlers = (Console.PreampModeChanged)Delegate.Remove(obj26.PreampModeChangedHandlers, new Console.PreampModeChanged(OnPreampModeChanged));
			Console obj27 = console;
			obj27.FMDeviationChangedHandlers = (Console.FMDeviationChanged)Delegate.Remove(obj27.FMDeviationChangedHandlers, new Console.FMDeviationChanged(OnFMDeviationChanged));
			Console obj28 = console;
			obj28.AGCGainChangedHandlers = (Console.AGCGainChanged)Delegate.Remove(obj28.AGCGainChangedHandlers, new Console.AGCGainChanged(OnAGCGainChanged));
			Console obj29 = console;
			obj29.RITChangedHandlers = (Console.RITChanged)Delegate.Remove(obj29.RITChangedHandlers, new Console.RITChanged(OnRITChanged));
			Console obj30 = console;
			obj30.XITChangedHandlers = (Console.XITChanged)Delegate.Remove(obj30.XITChangedHandlers, new Console.XITChanged(OnXITChanged));
			Console obj31 = console;
			obj31.RITValueChangedHandlers = (Console.RITValueChanged)Delegate.Remove(obj31.RITValueChangedHandlers, new Console.RITValueChanged(OnRITValueChanged));
			Console obj32 = console;
			obj32.XITValueChangedHandlers = (Console.XITValueChanged)Delegate.Remove(obj32.XITValueChangedHandlers, new Console.XITValueChanged(OnXITValueChanged));
			Console obj33 = console;
			obj33.TXFrequncyChangedHandlers = (Console.TXFrequncyChanged)Delegate.Remove(obj33.TXFrequncyChangedHandlers, new Console.TXFrequncyChanged(OnTXFrequencyChanged));
			Console obj34 = console;
			obj34.MeterReadingsChangedHandlers = (Console.MeterReadings)Delegate.Remove(obj34.MeterReadingsChangedHandlers, new Console.MeterReadings(OnMeterReadingsChanged));
			Console obj35 = console;
			obj35.NRChangedHandlers = (Console.NRChanged)Delegate.Remove(obj35.NRChangedHandlers, new Console.NRChanged(OnNrChanged));
			Console obj36 = console;
			obj36.NBChangedHandlers = (Console.NBChanged)Delegate.Remove(obj36.NBChangedHandlers, new Console.NBChanged(OnNbChanged));
			Console obj37 = console;
			obj37.ANFChangedHandlers = (Console.ANFChanged)Delegate.Remove(obj37.ANFChangedHandlers, new Console.ANFChanged(OnAnfChanged));
			Console obj38 = console;
			obj38.BINChangedHandlers = (Console.BINChanged)Delegate.Remove(obj38.BINChangedHandlers, new Console.BINChanged(OnBinChanged));
			Console obj39 = console;
			obj39.AGCModeChangedHandlers = (Console.AGCModeChanged)Delegate.Remove(obj39.AGCModeChangedHandlers, new Console.AGCModeChanged(OnAGCModeChanged));
			Console obj40 = console;
			obj40.AGCAutoModeChangedHandlers = (Console.AGCAutoModeChanged)Delegate.Remove(obj40.AGCAutoModeChangedHandlers, new Console.AGCAutoModeChanged(OnAGCAutoModeChanged));
			Console obj41 = console;
			obj41.VFOSyncChangedHandlers = (Console.VFOSyncChanged)Delegate.Remove(obj41.VFOSyncChangedHandlers, new Console.VFOSyncChanged(OnVFOSyncChanged));
			Console obj42 = console;
			obj42.VfoALockChangedHandlers = (Console.VfoALockChanged)Delegate.Remove(obj42.VfoALockChangedHandlers, new Console.VfoALockChanged(OnVfoALockChanged));
			Console obj43 = console;
			obj43.VfoBLockChangedHandlers = (Console.VfoBLockChanged)Delegate.Remove(obj43.VfoBLockChangedHandlers, new Console.VfoBLockChanged(OnVfoBLockChanged));
			Console obj44 = console;
			obj44.SQLChangedHandlers = (Console.SQLChanged)Delegate.Remove(obj44.SQLChangedHandlers, new Console.SQLChanged(OnSqlChanged));
			Console obj45 = console;
			obj45.SQLLevelChangedHandlers = (Console.SQLLevelChanged)Delegate.Remove(obj45.SQLLevelChangedHandlers, new Console.SQLLevelChanged(OnSqlLevelChanged));
			Console obj46 = console;
			obj46.APFChangedHandlers = (Console.APFChanged)Delegate.Remove(obj46.APFChangedHandlers, new Console.APFChanged(OnApfChanged));
			Console obj47 = console;
			obj47.TNFChangedHandlers = (Console.TNFChanged)Delegate.Remove(obj47.TNFChangedHandlers, new Console.TNFChanged(OnTnfChanged));
			Console obj48 = console;
			obj48.DIGLOffsetChangedHandlers = (Console.DIGLOffsetChanged)Delegate.Remove(obj48.DIGLOffsetChangedHandlers, new Console.DIGLOffsetChanged(OnDiglOffsetChanged));
			Console obj49 = console;
			obj49.DIGUOffsetChangedHandlers = (Console.DIGUOffsetChanged)Delegate.Remove(obj49.DIGUOffsetChangedHandlers, new Console.DIGUOffsetChanged(OnDiguOffsetChanged));
			Console obj50 = console;
			obj50.CWXSpeedChangedHandlers = (Console.CWXSpeedChanged)Delegate.Remove(obj50.CWXSpeedChangedHandlers, new Console.CWXSpeedChanged(OnCwMacrosSpeedChanged));
			Console obj51 = console;
			obj51.CWXDelayChangedHandlers = (Console.CWXDelayChanged)Delegate.Remove(obj51.CWXDelayChangedHandlers, new Console.CWXDelayChanged(OnCwMacrosDelayChanged));
			Console obj52 = console;
			obj52.CWXRemoteCharacterStartedHandlers = (Console.CWXRemoteCharacterStarted)Delegate.Remove(obj52.CWXRemoteCharacterStartedHandlers, new Console.CWXRemoteCharacterStarted(OnCwRemoteCharacterStarted));
			Console obj53 = console;
			obj53.CWKeyerSpeedChangedHandlers = (Console.CWKeyerSpeedChanged)Delegate.Remove(obj53.CWKeyerSpeedChangedHandlers, new Console.CWKeyerSpeedChanged(OnCwKeyerSpeedChanged));
			Console obj54 = console;
			obj54.RXGainChangedHandlers = (Console.RXGainChanged)Delegate.Remove(obj54.RXGainChangedHandlers, new Console.RXGainChanged(OnRxAfGainChanged));
			Console obj55 = console;
			obj55.CTUNChangedHandlers = (Console.CTUNChanged)Delegate.Remove(obj55.CTUNChangedHandlers, new Console.CTUNChanged(OnCTUNChanged));
			Console obj56 = console;
			obj56.TXProfileChangedHandlers = (Console.TXProfileChanged)Delegate.Remove(obj56.TXProfileChangedHandlers, new Console.TXProfileChanged(OnTXProfileChanged));
			Console obj57 = console;
			obj57.TXProfilesChangedHandlers = (Console.TXProfilesChanged)Delegate.Remove(obj57.TXProfilesChangedHandlers, new Console.TXProfilesChanged(OnTXProfilesChanged));
			Console obj58 = console;
			obj58.MeterCalOffsetChangedHandlers = (Console.MeterCalOffsetChanged)Delegate.Remove(obj58.MeterCalOffsetChangedHandlers, new Console.MeterCalOffsetChanged(OnCalibrationChanged));
			Console obj59 = console;
			obj59.DisplayOffsetChangedHandlers = (Console.DisplayOffsetChanged)Delegate.Remove(obj59.DisplayOffsetChangedHandlers, new Console.DisplayOffsetChanged(OnCalibrationChanged));
			Console obj60 = console;
			obj60.XvtrGainOffsetChangedHandlers = (Console.XvtrGainOffsetChanged)Delegate.Remove(obj60.XvtrGainOffsetChangedHandlers, new Console.XvtrGainOffsetChanged(OnCalibrationChanged));
			Console obj61 = console;
			obj61.Rx6mOffsetChangedHandlers = (Console.Rx6mOffsetChanged)Delegate.Remove(obj61.Rx6mOffsetChangedHandlers, new Console.Rx6mOffsetChanged(OnCalibrationChanged));
			m_bDelegatesAdded = false;
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
		StopAllSocketListers();
		cmaster.SetRXTCIRun(0);
		if (m_cwController != null)
		{
			m_cwController.Dispose();
			m_cwController = null;
		}
		m_server = null;
	}

	internal int GetCwMacrosSpeed()
	{
		if (m_cwController == null)
		{
			return 30;
		}
		return m_cwController.GetMacroSpeed();
	}

	internal void SetCwMacrosSpeed(int wpm)
	{
		m_cwController?.SetMacroSpeed(wpm);
	}

	internal int GetCwMacrosDelay()
	{
		if (m_cwController == null)
		{
			return 0;
		}
		return m_cwController.GetMacroDelayMs();
	}

	internal void SetCwMacrosDelay(int delayMs)
	{
		m_cwController?.SetMacroDelayMs(delayMs);
	}

	internal int GetCwKeyerSpeed()
	{
		if (m_cwController == null)
		{
			return 30;
		}
		return m_cwController.GetKeyerSpeed();
	}

	internal void SetCwKeyerSpeed(int wpm)
	{
		m_cwController?.SetKeyerSpeed(wpm);
	}

	internal void IncreaseCwMacrosSpeed(int amount)
	{
		m_cwController?.IncreaseMacroSpeed(amount);
	}

	internal void DecreaseCwMacrosSpeed(int amount)
	{
		m_cwController?.DecreaseMacroSpeed(amount);
	}

	internal void SetCwTerminalEnabled(TCPIPtciSocketListener socketListener, int rx, bool enabled)
	{
		m_cwController?.SetTerminalEnabled(socketListener, rx, enabled);
	}

	internal void SendCwMacro(TCPIPtciSocketListener socketListener, int rx, string text)
	{
		m_cwController?.SendMacro(socketListener, rx, text);
	}

	internal void SendCwMessage(TCPIPtciSocketListener socketListener, int rx, string prefix, string callsign, string suffix)
	{
		m_cwController?.SendMessage(socketListener, rx, prefix, callsign, suffix);
	}

	internal void UpdateCwMessageCallsign(TCPIPtciSocketListener socketListener, string callsign)
	{
		m_cwController?.UpdatePendingCallsign(socketListener, callsign);
	}

	internal void StopCwMacros(TCPIPtciSocketListener socketListener)
	{
		m_cwController?.Stop(socketListener);
	}

	internal void HandleCwKeyer(TCPIPtciSocketListener socketListener, int rx, bool pressed, int durationMs)
	{
		m_cwController?.HandleKeyer(socketListener, rx, pressed, durationMs);
	}

	internal void NotifyCwTciPttReleased(TCPIPtciSocketListener socketListener)
	{
		m_cwController?.HandleTciPttReleased(socketListener);
	}

	internal void OnSocketListenerDisconnected(TCPIPtciSocketListener socketListener)
	{
		m_cwController?.DisconnectClient(socketListener);
	}

	internal void OnCwMacrosEmpty(int rx)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CwMacrosEmpty(rx);
			}
		}
	}

	internal void OnCwCallsignSent(string callsign)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CwCallsignSent(callsign);
			}
		}
	}

	private void OnCwMacrosSpeedChanged(int oldSpeed, int newSpeed)
	{
		if (Interlocked.CompareExchange(ref m_cwInternalMacroSpeedUpdates, 0, 0) > 0)
		{
			return;
		}
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CwMacrosSpeedChanged(newSpeed);
			}
		}
	}

	private void OnCwMacrosDelayChanged(int oldDelay, int newDelay)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CwMacrosDelayChanged(newDelay);
			}
		}
	}

	private void OnCwRemoteCharacterStarted(int remainingRemoteCharacters, int pendingElements)
	{
		m_cwController?.OnRemoteCharacterStarted(remainingRemoteCharacters, pendingElements);
	}

	private void OnCwKeyerSpeedChanged(int oldSpeed, int newSpeed)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CwKeyerSpeedChanged(newSpeed);
			}
		}
	}

	private void StopAllSocketListers()
	{
		List<TCPIPtciSocketListener> list;
		lock (m_objLocker)
		{
			if (m_socketListenersList == null)
			{
				return;
			}
			list = new List<TCPIPtciSocketListener>(m_socketListenersList);
			m_socketListenersList.Clear();
			m_socketListenersList = null;
		}
		foreach (TCPIPtciSocketListener item in list)
		{
			item.ClientConnectedHandlers = (TCPIPtciSocketListener.ClientConnected)Delegate.Remove(item.ClientConnectedHandlers, new TCPIPtciSocketListener.ClientConnected(ClientConnectedHandler));
			item.ClientDisconnectedHandlers = (TCPIPtciSocketListener.ClientDisconnected)Delegate.Remove(item.ClientDisconnectedHandlers, new TCPIPtciSocketListener.ClientDisconnected(ClientDisconnectedHandler));
			item.ClientErrorHandlers = (TCPIPtciSocketListener.ClientError)Delegate.Remove(item.ClientErrorHandlers, new TCPIPtciSocketListener.ClientError(ClientErrorHandler));
			item.StopSocketListener();
		}
	}

	private void ServerThreadStart()
	{
		TCPIPtciSocketListener tCPIPtciSocketListener = null;
		bool flag = false;
		while (!m_stopServer)
		{
			try
			{
				flag = false;
				TcpClient tcpClient = m_server.AcceptTcpClient();
				tcpClient.NoDelay = true;
				tCPIPtciSocketListener = new TCPIPtciSocketListener(tcpClient, console, this, m_nRateLimit);
				lock (m_objLocker)
				{
					m_socketListenersList.Add(tCPIPtciSocketListener);
				}
				TCPIPtciSocketListener tCPIPtciSocketListener2 = tCPIPtciSocketListener;
				tCPIPtciSocketListener2.ClientConnectedHandlers = (TCPIPtciSocketListener.ClientConnected)Delegate.Combine(tCPIPtciSocketListener2.ClientConnectedHandlers, new TCPIPtciSocketListener.ClientConnected(ClientConnectedHandler));
				TCPIPtciSocketListener tCPIPtciSocketListener3 = tCPIPtciSocketListener;
				tCPIPtciSocketListener3.ClientDisconnectedHandlers = (TCPIPtciSocketListener.ClientDisconnected)Delegate.Combine(tCPIPtciSocketListener3.ClientDisconnectedHandlers, new TCPIPtciSocketListener.ClientDisconnected(ClientDisconnectedHandler));
				TCPIPtciSocketListener tCPIPtciSocketListener4 = tCPIPtciSocketListener;
				tCPIPtciSocketListener4.ClientErrorHandlers = (TCPIPtciSocketListener.ClientError)Delegate.Combine(tCPIPtciSocketListener4.ClientErrorHandlers, new TCPIPtciSocketListener.ClientError(ClientErrorHandler));
				flag = true;
				tCPIPtciSocketListener.StartSocketListener();
			}
			catch (SocketException ex)
			{
				if (flag && tCPIPtciSocketListener != null)
				{
					TCPIPtciSocketListener tCPIPtciSocketListener5 = tCPIPtciSocketListener;
					tCPIPtciSocketListener5.ClientConnectedHandlers = (TCPIPtciSocketListener.ClientConnected)Delegate.Remove(tCPIPtciSocketListener5.ClientConnectedHandlers, new TCPIPtciSocketListener.ClientConnected(ClientConnectedHandler));
					TCPIPtciSocketListener tCPIPtciSocketListener6 = tCPIPtciSocketListener;
					tCPIPtciSocketListener6.ClientDisconnectedHandlers = (TCPIPtciSocketListener.ClientDisconnected)Delegate.Remove(tCPIPtciSocketListener6.ClientDisconnectedHandlers, new TCPIPtciSocketListener.ClientDisconnected(ClientDisconnectedHandler));
					TCPIPtciSocketListener tCPIPtciSocketListener7 = tCPIPtciSocketListener;
					tCPIPtciSocketListener7.ClientErrorHandlers = (TCPIPtciSocketListener.ClientError)Delegate.Remove(tCPIPtciSocketListener7.ClientErrorHandlers, new TCPIPtciSocketListener.ClientError(ClientErrorHandler));
				}
				m_stopServer = true;
				m_sLastError = ex.Message;
				ServerErrorHandlers?.Invoke(ex);
			}
			catch
			{
				if (flag && tCPIPtciSocketListener != null)
				{
					TCPIPtciSocketListener tCPIPtciSocketListener8 = tCPIPtciSocketListener;
					tCPIPtciSocketListener8.ClientConnectedHandlers = (TCPIPtciSocketListener.ClientConnected)Delegate.Remove(tCPIPtciSocketListener8.ClientConnectedHandlers, new TCPIPtciSocketListener.ClientConnected(ClientConnectedHandler));
					TCPIPtciSocketListener tCPIPtciSocketListener9 = tCPIPtciSocketListener;
					tCPIPtciSocketListener9.ClientDisconnectedHandlers = (TCPIPtciSocketListener.ClientDisconnected)Delegate.Remove(tCPIPtciSocketListener9.ClientDisconnectedHandlers, new TCPIPtciSocketListener.ClientDisconnected(ClientDisconnectedHandler));
					TCPIPtciSocketListener tCPIPtciSocketListener10 = tCPIPtciSocketListener;
					tCPIPtciSocketListener10.ClientErrorHandlers = (TCPIPtciSocketListener.ClientError)Delegate.Remove(tCPIPtciSocketListener10.ClientErrorHandlers, new TCPIPtciSocketListener.ClientError(ClientErrorHandler));
				}
				m_stopServer = true;
			}
		}
	}

	private void PurgingThreadStart()
	{
		while (!m_stopPurging)
		{
			List<TCPIPtciSocketListener> list = new List<TCPIPtciSocketListener>();
			lock (m_objLocker)
			{
				if (m_server == null || m_socketListenersList == null)
				{
					break;
				}
				foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
				{
					if (socketListeners.IsMarkedForDeletion())
					{
						list.Add(socketListeners);
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					m_socketListenersList.Remove(list[i]);
				}
			}
			foreach (TCPIPtciSocketListener item in list)
			{
				item.ClientConnectedHandlers = (TCPIPtciSocketListener.ClientConnected)Delegate.Remove(item.ClientConnectedHandlers, new TCPIPtciSocketListener.ClientConnected(ClientConnectedHandler));
				item.ClientDisconnectedHandlers = (TCPIPtciSocketListener.ClientDisconnected)Delegate.Remove(item.ClientDisconnectedHandlers, new TCPIPtciSocketListener.ClientDisconnected(ClientDisconnectedHandler));
				item.ClientErrorHandlers = (TCPIPtciSocketListener.ClientError)Delegate.Remove(item.ClientErrorHandlers, new TCPIPtciSocketListener.ClientError(ClientErrorHandler));
				item.StopSocketListener();
			}
			list = null;
			m_bSleepingInPurge = true;
			Thread.Sleep(5000);
			m_bSleepingInPurge = false;
		}
	}

	private void ClientConnectedHandler()
	{
		RefreshStreamRunState();
		ClientConnectedHandlers?.Invoke();
	}

	private void ClientDisconnectedHandler()
	{
		RefreshStreamRunState();
		ClientDisconnectedHandlers?.Invoke();
	}

	private void ClientErrorHandler(SocketException se)
	{
		m_sLastError = se.Message;
		ClientErrorHandlers?.Invoke(se);
	}

	public void OnVFOAFrequencyChangeHandler(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
	{
		bool flag = console != null && console.RX2Enabled && UseRX1VFOaForRX2VFOa;
		TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData
		{
			cen = false,
			centreMHz = -1.0,
			rx = (flag ? 1 : (rx - 1)),
			freqMHz = newFreq,
			offsetHz = (int)(0.0 - offset),
			chan = 0,
			duplicate_tochan = -1,
			replace_if_duplicated = false,
			sendIF = true
		};
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.VFOChange(vfod);
			}
		}
	}

	public void OnVFOBFrequencyChangeHandler(Band oldBand, Band newBand, DSPMode oldMode, DSPMode newMode, Filter oldFilter, Filter newFilter, double oldFreq, double newFreq, double oldCentreF, double newCentreF, bool oldCTUN, bool newCTUN, int oldZoomSlider, int newZoomSlider, double offset, int rx)
	{
		TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData
		{
			cen = false,
			centreMHz = -1.0,
			rx = rx - 1,
			freqMHz = newFreq,
			offsetHz = (int)(0.0 - offset),
			chan = 1,
			duplicate_tochan = ((!m_bCopyRX2VFObToVFOa || !console.RX2Enabled) ? (-1) : 0),
			replace_if_duplicated = (m_bCopyRX2VFObToVFOa && _replace_if_copy_RX2VFObToVFOa && console.RX2Enabled),
			sendIF = true
		};
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.VFOChange(vfod);
			}
		}
	}

	public void OnMoxChangeHandler(int rx, bool oldMox, bool newMox)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.MoxChange(rx, oldMox, newMox);
				socketListeners.SyncTciPttToMox(newMox);
			}
		}
		RefreshTxAudioSourceState();
	}

	public void OnMoxPreChangeHandler(int rx, bool currentMox, bool expectedMox)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.SyncTciPttToMox(expectedMox);
			}
		}
		RefreshTxAudioSourceState();
	}

	public void OnModeChangeHandler(int rx, DSPMode oldMode, DSPMode newMode, Band oldBand, Band newBand)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.ModeChange(rx, oldMode, newMode, oldBand, newBand);
			}
		}
	}

	public void OnBandChangeHandler(int rx, Band oldBand, Band newBand)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.BandChange(rx, oldBand, newBand);
			}
		}
	}

	public void OnCentreFrequencyChanged(int rx, double oldFreq, double newFreq, Band band, double offset)
	{
		bool flag = ((rx == 1) ? console.ClickTuneDisplay : console.ClickTuneRX2Display);
		TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData
		{
			freqMHz = -1.0,
			offsetHz = (flag ? ((int)(0.0 - offset)) : (-1)),
			chan = 0,
			centreMHz = newFreq,
			cen = true,
			rx = rx - 1,
			duplicate_tochan = -1,
			replace_if_duplicated = false,
			sendIF = flag
		};
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CentreChange(vfod);
			}
		}
	}

	public void OnFilterChanged(int rx, Filter oldFilter, Filter newFilter, Band band, int low, int high, string sName)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.FilterChange(rx, oldFilter, newFilter, band, low, high);
			}
		}
	}

	public void OnFilterEdgesChanged(int rx, Filter filter, Band band, int low, int high, string sName, int max_width, int max_shift)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.FilterEdgesChange(rx, filter, band, low, high);
			}
		}
	}

	public void OnTXFiltersChanged(int low, int high)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.TXFilterBandChanged(low, high);
			}
		}
	}

	public void OnPowerChangeHander(bool oldPower, bool newPower)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.PowerChange(oldPower, newPower);
			}
		}
	}

	public void OnThetisFocusChanged(bool focus)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.ThetisFocusChange(focus);
			}
		}
	}

	public void OnRX2EnabledChanged(bool enabled)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RX2EnabledChange(enabled);
			}
		}
	}

	private void OnHWSampleRateChanged(int rx, int oldSampleRate, int newSampleRate)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.HWSampleRateChange(rx, oldSampleRate, newSampleRate);
			}
		}
	}

	private void OnDrivePowerChanged(int rx, int newPower, bool tune)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.DrivePowerChange(rx, newPower, tune);
			}
		}
	}

	private void OnTuneChanged(int rx, bool oldTune, bool newTune)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.TuneChange(rx, oldTune, newTune);
			}
		}
	}

	private void OnSplitChanged(int rx, bool oldSplit, bool newSplit)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.SplitChange(rx, newSplit);
			}
		}
	}

	private void OnSpotClicked(string callsign, long frequencyHz, int rx = -1, bool vfoB = false)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.ClickedOnSpot(callsign, frequencyHz);
				socketListeners.ClickedOnSpot(callsign, frequencyHz, rx, vfoB ? 1 : 0);
			}
		}
	}

	private void OnMuteChanged(int rx, bool oldState, bool newState)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.MuteChanged(rx, newState);
			}
		}
	}

	private void OnNrChanged(int rx, int old_nr, int new_nr)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.NRChanged(rx, new_nr);
			}
		}
	}

	private void OnNbChanged(int rx, int old_nb, int new_nb)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.NBChanged(rx, new_nb);
			}
		}
	}

	private void OnAnfChanged(int rx, bool old_state, bool new_state)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.AnfChanged(rx, new_state);
			}
		}
	}

	private void OnBinChanged(int rx, bool old_state, bool new_state)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.BinChanged(rx, new_state);
			}
		}
	}

	private void OnAGCModeChanged(int rx, AGCMode old_mode, AGCMode new_mode)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.AGCModeChanged(rx, new_mode);
			}
		}
	}

	private void OnAGCAutoModeChanged(int rx, bool old_state, bool new_state)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.AGCAutoChanged(rx, new_state);
			}
		}
	}

	private void OnVFOSyncChanged(int rx, bool old_state, bool new_state)
	{
		if (rx != 1)
		{
			return;
		}
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.VFOSyncChanged(new_state);
			}
		}
	}

	private void OnVfoALockChanged(int rx, bool old_state, bool new_state)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.LockChanged(1, new_state);
				socketListeners.VFOLocksChanged();
			}
		}
	}

	private void OnVfoBLockChanged(int rx, bool old_state, bool new_state)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				if (rx == 2)
				{
					socketListeners.LockChanged(2, new_state);
				}
				socketListeners.VFOLocksChanged();
			}
		}
	}

	private void OnSqlChanged(int rx, SquelchState old_state, SquelchState new_state)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.SqlChanged(rx, new_state);
			}
		}
	}

	private void OnSqlLevelChanged(int rx, int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.SqlLevelChanged(rx, newValue);
			}
		}
	}

	private void OnApfChanged(int rx, bool oldState, bool newState)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.ApfChanged(rx, newState);
			}
		}
	}

	private void OnTnfChanged(bool old_tnf, bool new_tnf)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.NfChanged(new_tnf);
			}
		}
	}

	private void OnDiglOffsetChanged(int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.DiglOffsetChanged(newValue);
			}
		}
	}

	private void OnDiguOffsetChanged(int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.DiguOffsetChanged(newValue);
			}
		}
	}

	private void OnRxAfGainChanged(int rx, bool is_subrx, int old_gain, int new_gain)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RxAfGainChanged(rx, is_subrx, new_gain);
			}
		}
	}

	private void OnCTUNChanged(int rx, bool oldCTUN, bool newCTUN, Band band)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CTUNChanged(rx, newCTUN);
			}
		}
	}

	private void OnTXProfileChanged(string old_name, string new_name)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.TXProfileChanged(new_name);
			}
		}
	}

	private void OnTXProfilesChanged()
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.TXProfilesChanged();
			}
		}
	}

	private void OnCalibrationChanged(int rx, float oldcal, float newcal)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.CalibrationChanged(rx);
			}
		}
	}

	private void OnMONChanged(bool oldState, bool newState)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.MONChanged(newState);
			}
		}
	}

	private void OnMONVolumeChanged(int oldVolume, int newVolume)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.MONVolumeChanged(newVolume);
			}
		}
	}

	private void OnVolumeChanged(int oldVolume, int newVolume)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.VolumeChanged(newVolume);
			}
		}
	}

	private void OnBalanceChanged(int rx, bool is_subrx, int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.BalanceChanged(rx, is_subrx, newValue);
			}
		}
	}

	private void OnAttenuatorDataChanged(int rx, int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RxStepAttChanged(rx, newValue);
			}
		}
	}

	private void OnStepAttEnabledChanged(int rx, bool oldEnabled, bool newEnabled)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RxStepAttEnabledChanged(rx, newEnabled);
			}
		}
	}

	private void OnPreampModeChanged(int rx, PreampMode oldMode, PreampMode newMode)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RxPreampAttChanged(rx, newMode);
			}
		}
	}

	private void OnFMDeviationChanged(int rx, int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.FMDeviationChanged(rx, newValue);
			}
		}
	}

	private void OnAGCGainChanged(int rx, int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.AGCGainChanged(rx, newValue);
			}
		}
	}

	private void OnRITChanged(bool oldState, bool newState)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RITChanged(newState);
			}
		}
	}

	private void OnXITChanged(bool oldState, bool newState)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.XITChanged(newState);
			}
		}
	}

	private void OnRITValueChanged(int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.RITValueChanged(newValue);
			}
		}
	}

	private void OnXITValueChanged(int oldValue, int newValue)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.XITValueChanged(newValue);
			}
		}
	}

	private void OnTXFrequencyChanged(double old_frequency, double new_frequency, Band old_band, Band new_band, bool rx2_enabled, bool tx_vfob, double centre_freq)
	{
		TCPIPtciSocketListener.VFOData vfod = new TCPIPtciSocketListener.VFOData
		{
			cen = false,
			centreMHz = -1.0,
			rx = ((rx2_enabled & tx_vfob) ? 1 : 0),
			freqMHz = new_frequency,
			offsetHz = -1,
			chan = -1,
			duplicate_tochan = -1,
			replace_if_duplicated = false,
			sendIF = false,
			SendTXInfo = true,
			TXfreqBand = new_band,
			RX2EnabledForTX = rx2_enabled,
			TXVFOB = tx_vfob
		};
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.TXFrequencyChange(vfod);
			}
		}
	}

	private void OnMeterReadingsChanged(int rx, bool tx, ref Dictionary<Reading, float> readings)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.MeterReadingsChanged(rx, tx, ref readings);
			}
		}
	}

	public void ShowLog()
	{
		if (_log != null)
		{
			_log.ShowWithTitle("TCI");
		}
	}

	public void CloseLog()
	{
		if (_log != null)
		{
			_log.Hide();
		}
	}

	public void SendSpotSimulationClickToAll(string callsign, long freq)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.ClickedOnSpot(callsign, freq);
				socketListeners.ClickedOnSpot(callsign, freq, 1, 0);
			}
		}
	}

	internal void RefreshStreamRunState()
	{
		bool flag = false;
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			bool flag2 = false;
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				if (socketListeners != null && socketListeners.IsReadyForStreaming())
				{
					flag2 = true;
					if (socketListeners.WantsAnyRxStream())
					{
						flag = true;
						break;
					}
				}
			}
			if ((!flag & flag2) && m_bAlwaysStreamIQ)
			{
				flag = true;
			}
		}
		cmaster.SetRXTCIRun(flag ? 1 : 0);
	}

	public void PublishIQSamples(int receiver, int sampleRate, float[] iqSamples, int complexSamples = -1)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.PublishIQSamples(receiver, sampleRate, iqSamples, complexSamples);
			}
		}
	}

	public void PublishRxAudioSamples(int receiver, int sampleRate, float[] left, float[] right, int samples = -1)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				socketListeners.PublishRxAudioSamples(receiver, sampleRate, left, right, samples);
			}
		}
	}

	public bool RequiresRxSensorUpdate(int receiver, int channel)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return false;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				if (socketListeners != null && socketListeners.RequiresRxSensorUpdate(receiver, channel))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool SensorRequiresUpdate(int receiver, Reading reading)
	{
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return false;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				if (socketListeners != null && socketListeners.SensorRequiresUpdate(receiver, reading))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int MinimumRequiredRxSensorInterval()
	{
		int num = int.MaxValue;
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return num;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				if (socketListeners != null)
				{
					int num2 = socketListeners.MinimumRequiredRxSensorInterval();
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
			return num;
		}
	}

	public int MinimumRequiredTxSensorInterval()
	{
		int num = int.MaxValue;
		lock (m_objLocker)
		{
			if (m_server == null || m_socketListenersList == null)
			{
				return num;
			}
			foreach (TCPIPtciSocketListener socketListeners in m_socketListenersList)
			{
				if (socketListeners != null)
				{
					int num2 = socketListeners.MinimumRequiredTxSensorInterval();
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
			return num;
		}
	}

	private TCPIPtciSocketListener GetActiveTxAudioListener()
	{
		if (m_server == null || m_socketListenersList == null)
		{
			m_activeTxAudioListener = null;
			return null;
		}
		if (m_activeTxAudioListener != null && !m_socketListenersList.Contains(m_activeTxAudioListener))
		{
			m_activeTxAudioListener = null;
		}
		return m_activeTxAudioListener;
	}

	internal bool TryAcquireActiveTxAudioListener(TCPIPtciSocketListener socketListener)
	{
		lock (m_objLocker)
		{
			TCPIPtciSocketListener tCPIPtciSocketListener = GetActiveTxAudioListener();
			if (tCPIPtciSocketListener != null && !tCPIPtciSocketListener.UsesActiveTCITxAudio())
			{
				m_activeTxAudioListener = null;
				tCPIPtciSocketListener = null;
			}
			if (tCPIPtciSocketListener == null || tCPIPtciSocketListener == socketListener)
			{
				m_activeTxAudioListener = socketListener;
				return true;
			}
		}
		return false;
	}

	internal void ReleaseActiveTxAudioListener(TCPIPtciSocketListener socketListener)
	{
		lock (m_objLocker)
		{
			if (m_activeTxAudioListener == socketListener)
			{
				m_activeTxAudioListener = null;
			}
		}
	}

	internal bool UsesActiveTCITxAudio()
	{
		lock (m_objLocker)
		{
			return GetActiveTxAudioListener()?.UsesActiveTCITxAudio() ?? false;
		}
	}

	internal bool TryGetTxAudioRequestSettings(out int sampleRate, out int samples, out int bufferingMs)
	{
		sampleRate = 0;
		samples = 0;
		bufferingMs = 0;
		lock (m_objLocker)
		{
			return GetActiveTxAudioListener()?.TryGetTxAudioRequestSettings(out sampleRate, out samples, out bufferingMs) ?? false;
		}
	}

	internal void RefreshTxAudioSourceState()
	{
		cmaster.SetTXTCIAudioRun(0, UsesActiveTCITxAudio() ? 1 : 0);
	}

	public void SendTxChrono(int receiver)
	{
		lock (m_objLocker)
		{
			GetActiveTxAudioListener()?.SendTxChrono(receiver);
		}
	}

	internal bool TryDequeueTxAudio(out TCIQueuedTxAudio queuedAudio)
	{
		lock (m_objLocker)
		{
			TCPIPtciSocketListener activeTxAudioListener = GetActiveTxAudioListener();
			if (activeTxAudioListener != null && activeTxAudioListener.TryDequeueTxAudio(out queuedAudio))
			{
				return true;
			}
		}
		queuedAudio = null;
		return false;
	}
}
