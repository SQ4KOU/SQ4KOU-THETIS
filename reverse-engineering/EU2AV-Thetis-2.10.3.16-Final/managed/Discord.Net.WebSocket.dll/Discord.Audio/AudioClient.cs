using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.API.Voice;
using Discord.Audio.Streams;
using Discord.Logging;
using Discord.Net.Converters;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Discord.Audio;

internal class AudioClient : IAudioClient, IDisposable
{
	private struct StreamPair(AudioInStream reader, AudioOutStream writer)
	{
		public AudioInStream Reader = reader;

		public AudioOutStream Writer = writer;
	}

	internal enum StopReason
	{
		Unknown,
		Normal,
		Disconnected,
		Moved
	}

	private static readonly int ConnectionTimeoutMs = 30000;

	private static readonly int KeepAliveIntervalMs = 5000;

	private static readonly int[] BlacklistedResumeCodes = new int[10] { 4001, 4002, 4003, 4004, 4005, 4006, 4009, 4012, 1014, 4016 };

	private readonly Logger _audioLogger;

	private readonly JsonSerializer _serializer;

	private readonly ConnectionManager _connection;

	private readonly SemaphoreSlim _stateLock;

	private readonly ConcurrentQueue<long> _heartbeatTimes;

	private readonly ConcurrentQueue<KeyValuePair<ulong, int>> _keepaliveTimes;

	private readonly ConcurrentDictionary<uint, ulong> _ssrcMap;

	private readonly ConcurrentDictionary<ulong, StreamPair> _streams;

	private Task _heartbeatTask;

	private Task _keepaliveTask;

	private int _heartbeatInterval;

	private long _lastMessageTime;

	private string _url;

	private string _sessionId;

	private string _token;

	private ulong _userId;

	private uint _ssrc;

	private bool _isSpeaking;

	private StopReason _stopReason;

	private bool _resuming;

	private readonly AsyncEvent<Func<Task>> _connectedEvent = new AsyncEvent<Func<Task>>();

	private readonly AsyncEvent<Func<Exception, Task>> _disconnectedEvent = new AsyncEvent<Func<Exception, Task>>();

	private readonly AsyncEvent<Func<int, int, Task>> _latencyUpdatedEvent = new AsyncEvent<Func<int, int, Task>>();

	private readonly AsyncEvent<Func<int, int, Task>> _udpLatencyUpdatedEvent = new AsyncEvent<Func<int, int, Task>>();

	private readonly AsyncEvent<Func<ulong, AudioInStream, Task>> _streamCreatedEvent = new AsyncEvent<Func<ulong, AudioInStream, Task>>();

	private readonly AsyncEvent<Func<ulong, Task>> _streamDestroyedEvent = new AsyncEvent<Func<ulong, Task>>();

	private readonly AsyncEvent<Func<ulong, bool, Task>> _speakingUpdatedEvent = new AsyncEvent<Func<ulong, bool, Task>>();

	private readonly AsyncEvent<Func<ulong, Task>> _clientDisconnectedEvent = new AsyncEvent<Func<ulong, Task>>();

	public SocketGuild Guild { get; }

	public DiscordVoiceAPIClient ApiClient { get; private set; }

	public int Latency { get; private set; }

	public int UdpLatency { get; private set; }

	public ulong ChannelId { get; internal set; }

	internal byte[] SecretKey { get; private set; }

	internal bool IsFinished { get; private set; }

	private DiscordSocketClient Discord => Guild.Discord;

	public ConnectionState ConnectionState => _connection.State;

	public event Func<Task> Connected
	{
		add
		{
			_connectedEvent.Add(value);
		}
		remove
		{
			_connectedEvent.Remove(value);
		}
	}

	public event Func<Exception, Task> Disconnected
	{
		add
		{
			_disconnectedEvent.Add(value);
		}
		remove
		{
			_disconnectedEvent.Remove(value);
		}
	}

	public event Func<int, int, Task> LatencyUpdated
	{
		add
		{
			_latencyUpdatedEvent.Add(value);
		}
		remove
		{
			_latencyUpdatedEvent.Remove(value);
		}
	}

	public event Func<int, int, Task> UdpLatencyUpdated
	{
		add
		{
			_udpLatencyUpdatedEvent.Add(value);
		}
		remove
		{
			_udpLatencyUpdatedEvent.Remove(value);
		}
	}

	public event Func<ulong, AudioInStream, Task> StreamCreated
	{
		add
		{
			_streamCreatedEvent.Add(value);
		}
		remove
		{
			_streamCreatedEvent.Remove(value);
		}
	}

	public event Func<ulong, Task> StreamDestroyed
	{
		add
		{
			_streamDestroyedEvent.Add(value);
		}
		remove
		{
			_streamDestroyedEvent.Remove(value);
		}
	}

	public event Func<ulong, bool, Task> SpeakingUpdated
	{
		add
		{
			_speakingUpdatedEvent.Add(value);
		}
		remove
		{
			_speakingUpdatedEvent.Remove(value);
		}
	}

	public event Func<ulong, Task> ClientDisconnected
	{
		add
		{
			_clientDisconnectedEvent.Add(value);
		}
		remove
		{
			_clientDisconnectedEvent.Remove(value);
		}
	}

	internal AudioClient(SocketGuild guild, int clientId, ulong channelId)
	{
		Guild = guild;
		ChannelId = channelId;
		_audioLogger = Discord.LogManager.CreateLogger($"Audio #{clientId}");
		ApiClient = new DiscordVoiceAPIClient(guild.Id, Discord.WebSocketProvider, Discord.UdpSocketProvider);
		ApiClient.SentGatewayMessage += async delegate(VoiceOpCode opCode)
		{
			await _audioLogger.DebugAsync($"Sent {opCode}").ConfigureAwait(continueOnCapturedContext: false);
		};
		ApiClient.SentDiscovery += async delegate
		{
			await _audioLogger.DebugAsync("Sent Discovery").ConfigureAwait(continueOnCapturedContext: false);
		};
		ApiClient.ReceivedEvent += ProcessMessageAsync;
		ApiClient.ReceivedPacket += ProcessPacketAsync;
		_stateLock = new SemaphoreSlim(1, 1);
		_connection = new ConnectionManager(_stateLock, _audioLogger, ConnectionTimeoutMs, OnConnectingAsync, OnDisconnectingAsync, delegate(Func<Exception, Task> x)
		{
			ApiClient.Disconnected += x;
		});
		_connection.Connected += () => _connectedEvent.InvokeAsync();
		_connection.Disconnected += (Exception exception, bool _) => _disconnectedEvent.InvokeAsync(exception);
		_heartbeatTimes = new ConcurrentQueue<long>();
		_keepaliveTimes = new ConcurrentQueue<KeyValuePair<ulong, int>>();
		_ssrcMap = new ConcurrentDictionary<uint, ulong>();
		_streams = new ConcurrentDictionary<ulong, StreamPair>();
		_serializer = new JsonSerializer
		{
			ContractResolver = new DiscordContractResolver()
		};
		_serializer.Error += delegate(object s, ErrorEventArgs e)
		{
			_audioLogger.WarningAsync(e.ErrorContext.Error).GetAwaiter().GetResult();
			e.ErrorContext.Handled = true;
		};
		LatencyUpdated += async delegate(int old, int val)
		{
			await _audioLogger.DebugAsync($"Latency = {val} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
		UdpLatencyUpdated += async delegate(int old, int val)
		{
			await _audioLogger.DebugAsync($"UDP Latency = {val} ms").ConfigureAwait(continueOnCapturedContext: false);
		};
	}

	internal Task StartAsync(string url, ulong userId, string sessionId, string token)
	{
		_url = url;
		_userId = userId;
		_sessionId = sessionId;
		_token = token;
		return _connection.StartAsync();
	}

	public IReadOnlyDictionary<ulong, AudioInStream> GetStreams()
	{
		return _streams.ToDictionary((KeyValuePair<ulong, StreamPair> pair) => pair.Key, (KeyValuePair<ulong, StreamPair> pair) => pair.Value.Reader);
	}

	public Task StopAsync()
	{
		return StopAsync(StopReason.Normal);
	}

	internal Task StopAsync(StopReason stopReason)
	{
		_stopReason = stopReason;
		return _connection.StopAsync();
	}

	private async Task OnConnectingAsync()
	{
		await _audioLogger.DebugAsync("Connecting ApiClient. Voice server: wss://" + _url).ConfigureAwait(continueOnCapturedContext: false);
		await ApiClient.ConnectAsync($"wss://{_url}?v={3}").ConfigureAwait(continueOnCapturedContext: false);
		await _audioLogger.DebugAsync($"Listening on port {ApiClient.UdpPort}").ConfigureAwait(continueOnCapturedContext: false);
		if (_resuming)
		{
			await _audioLogger.DebugAsync("Sending Resume").ConfigureAwait(continueOnCapturedContext: false);
			await ApiClient.SendResume(_token, _sessionId).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await _audioLogger.DebugAsync("Sending Identity").ConfigureAwait(continueOnCapturedContext: false);
			await ApiClient.SendIdentityAsync(_userId, _sessionId, _token).ConfigureAwait(continueOnCapturedContext: false);
		}
		await _connection.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task OnDisconnectingAsync(Exception ex)
	{
		await _audioLogger.DebugAsync("Disconnecting ApiClient").ConfigureAwait(continueOnCapturedContext: false);
		await ApiClient.DisconnectAsync().ConfigureAwait(continueOnCapturedContext: false);
		if (_stopReason == StopReason.Unknown)
		{
			Exception innerException = ex.InnerException;
			if (innerException is WebSocketException exception)
			{
				await _audioLogger.WarningAsync($"Audio connection terminated with unknown reason. Code: {exception.ErrorCode} - {exception.Message}", exception);
				if (_resuming)
				{
					await _audioLogger.WarningAsync("Resume failed");
					_resuming = false;
					await FinishDisconnect(ex, wontTryReconnect: true);
				}
				else if (Enumerable.Contains(BlacklistedResumeCodes, exception.ErrorCode))
				{
					await FinishDisconnect(ex, wontTryReconnect: true);
				}
				else
				{
					await ClearHeartBeaters();
					_resuming = true;
				}
				return;
			}
		}
		await FinishDisconnect(ex, _stopReason != StopReason.Moved);
		if (_stopReason == StopReason.Normal)
		{
			await _audioLogger.DebugAsync("Sending Voice State").ConfigureAwait(continueOnCapturedContext: false);
			await Discord.ApiClient.SendVoiceStateUpdateAsync(Guild.Id, null, selfDeaf: false, selfMute: false).ConfigureAwait(continueOnCapturedContext: false);
		}
		_stopReason = StopReason.Unknown;
	}

	private async Task FinishDisconnect(Exception ex, bool wontTryReconnect)
	{
		await _audioLogger.DebugAsync("Finishing audio connection").ConfigureAwait(continueOnCapturedContext: false);
		await ClearHeartBeaters().ConfigureAwait(continueOnCapturedContext: false);
		if (wontTryReconnect)
		{
			await _connection.StopAsync().ConfigureAwait(continueOnCapturedContext: false);
			await ClearInputStreamsAsync().ConfigureAwait(continueOnCapturedContext: false);
			IsFinished = true;
		}
	}

	private async Task ClearHeartBeaters()
	{
		await _audioLogger.DebugAsync("Waiting for heartbeater").ConfigureAwait(continueOnCapturedContext: false);
		if (_heartbeatTask != null)
		{
			await _heartbeatTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_heartbeatTask = null;
		if (_keepaliveTask != null)
		{
			await _keepaliveTask.ConfigureAwait(continueOnCapturedContext: false);
		}
		_keepaliveTask = null;
		long result;
		while (_heartbeatTimes.TryDequeue(out result))
		{
		}
		_lastMessageTime = 0L;
		KeyValuePair<ulong, int> result2;
		while (_keepaliveTimes.TryDequeue(out result2))
		{
		}
	}

	public AudioOutStream CreateOpusStream(int bufferMillis)
	{
		return new BufferedWriteStream(new RTPWriteStream(new SodiumEncryptStream(new OutputStream(ApiClient), this), _ssrc), this, bufferMillis, _connection.CancelToken, _audioLogger);
	}

	public AudioOutStream CreateDirectOpusStream()
	{
		return new RTPWriteStream(new SodiumEncryptStream(new OutputStream(ApiClient), this), _ssrc);
	}

	public AudioOutStream CreatePCMStream(AudioApplication application, int? bitrate, int bufferMillis, int packetLoss)
	{
		return new OpusEncodeStream(new BufferedWriteStream(new RTPWriteStream(new SodiumEncryptStream(new OutputStream(ApiClient), this), _ssrc), this, bufferMillis, _connection.CancelToken, _audioLogger), bitrate ?? 98304, application, packetLoss);
	}

	public AudioOutStream CreateDirectPCMStream(AudioApplication application, int? bitrate, int packetLoss)
	{
		return new OpusEncodeStream(new RTPWriteStream(new SodiumEncryptStream(new OutputStream(ApiClient), this), _ssrc), bitrate ?? 98304, application, packetLoss);
	}

	internal async Task CreateInputStreamAsync(ulong userId)
	{
		if (!_streams.ContainsKey(userId))
		{
			InputStream inputStream = new InputStream();
			SodiumDecryptStream writer = new SodiumDecryptStream(new RTPReadStream(new OpusDecodeStream(inputStream)), this);
			_streams.TryAdd(userId, new StreamPair(inputStream, writer));
			await _streamCreatedEvent.InvokeAsync(userId, inputStream);
		}
	}

	internal AudioInStream GetInputStream(ulong id)
	{
		if (_streams.TryGetValue(id, out var value))
		{
			return value.Reader;
		}
		return null;
	}

	internal async Task RemoveInputStreamAsync(ulong userId)
	{
		if (_streams.TryRemove(userId, out var pair))
		{
			await _streamDestroyedEvent.InvokeAsync(userId).ConfigureAwait(continueOnCapturedContext: false);
			pair.Reader.Dispose();
		}
	}

	internal async Task ClearInputStreamsAsync()
	{
		foreach (KeyValuePair<ulong, StreamPair> pair in _streams)
		{
			await _streamDestroyedEvent.InvokeAsync(pair.Key).ConfigureAwait(continueOnCapturedContext: false);
			pair.Value.Reader.Dispose();
		}
		_ssrcMap.Clear();
		_streams.Clear();
	}

	private async Task ProcessMessageAsync(VoiceOpCode opCode, object payload)
	{
		_lastMessageTime = Environment.TickCount;
		try
		{
			switch (opCode)
			{
			case VoiceOpCode.Ready:
			{
				await _audioLogger.DebugAsync("Received Ready").ConfigureAwait(continueOnCapturedContext: false);
				ReadyEvent readyEvent = (payload as JToken).ToObject<ReadyEvent>(_serializer);
				_ssrc = readyEvent.SSRC;
				if (!Enumerable.Contains(readyEvent.Modes, "xsalsa20_poly1305"))
				{
					throw new InvalidOperationException("Discord does not support xsalsa20_poly1305");
				}
				ApiClient.SetUdpEndpoint(readyEvent.Ip, readyEvent.Port);
				await ApiClient.SendDiscoveryAsync(_ssrc).ConfigureAwait(continueOnCapturedContext: false);
				_heartbeatTask = RunHeartbeatAsync(_heartbeatInterval, _connection.CancelToken);
				break;
			}
			case VoiceOpCode.SessionDescription:
			{
				await _audioLogger.DebugAsync("Received SessionDescription").ConfigureAwait(continueOnCapturedContext: false);
				SessionDescriptionEvent sessionDescriptionEvent = (payload as JToken).ToObject<SessionDescriptionEvent>(_serializer);
				if (sessionDescriptionEvent.Mode != "xsalsa20_poly1305")
				{
					throw new InvalidOperationException("Discord selected an unexpected mode: " + sessionDescriptionEvent.Mode);
				}
				SecretKey = sessionDescriptionEvent.SecretKey;
				_isSpeaking = false;
				await ApiClient.SendSetSpeaking(_isSpeaking).ConfigureAwait(continueOnCapturedContext: false);
				_keepaliveTask = RunKeepaliveAsync(_connection.CancelToken);
				_connection.CompleteAsync();
				break;
			}
			case VoiceOpCode.HeartbeatAck:
			{
				await _audioLogger.DebugAsync("Received HeartbeatAck").ConfigureAwait(continueOnCapturedContext: false);
				if (_heartbeatTimes.TryDequeue(out var result))
				{
					int num = (int)(Environment.TickCount - result);
					int latency = Latency;
					Latency = num;
					await _latencyUpdatedEvent.InvokeAsync(latency, num).ConfigureAwait(continueOnCapturedContext: false);
				}
				break;
			}
			case VoiceOpCode.Hello:
			{
				await _audioLogger.DebugAsync("Received Hello").ConfigureAwait(continueOnCapturedContext: false);
				HelloEvent helloEvent = (payload as JToken).ToObject<HelloEvent>(_serializer);
				_heartbeatInterval = helloEvent.HeartbeatInterval;
				break;
			}
			case VoiceOpCode.Speaking:
			{
				await _audioLogger.DebugAsync("Received Speaking").ConfigureAwait(continueOnCapturedContext: false);
				SpeakingEvent speakingEvent = (payload as JToken).ToObject<SpeakingEvent>(_serializer);
				_ssrcMap[speakingEvent.Ssrc] = speakingEvent.UserId;
				await _speakingUpdatedEvent.InvokeAsync(speakingEvent.UserId, speakingEvent.Speaking);
				break;
			}
			case VoiceOpCode.ClientDisconnect:
			{
				await _audioLogger.DebugAsync("Received ClientDisconnect").ConfigureAwait(continueOnCapturedContext: false);
				ClientDisconnectEvent clientDisconnectEvent = (payload as JToken).ToObject<ClientDisconnectEvent>(_serializer);
				await _clientDisconnectedEvent.InvokeAsync(clientDisconnectEvent.UserId);
				break;
			}
			case VoiceOpCode.Resumed:
				await _audioLogger.DebugAsync("Voice connection resumed: wss://" + _url);
				_resuming = false;
				_heartbeatTask = RunHeartbeatAsync(_heartbeatInterval, _connection.CancelToken);
				_keepaliveTask = RunKeepaliveAsync(_connection.CancelToken);
				_connection.CompleteAsync();
				break;
			default:
				await _audioLogger.WarningAsync($"Unknown OpCode ({opCode})").ConfigureAwait(continueOnCapturedContext: false);
				break;
			}
		}
		catch (Exception exception)
		{
			await _audioLogger.ErrorAsync($"Error handling {opCode}", exception).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task ProcessPacketAsync(byte[] packet)
	{
		try
		{
			if (_connection.State == ConnectionState.Connecting)
			{
				if (packet.Length != 74)
				{
					await _audioLogger.DebugAsync("Malformed Packet").ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				string ip;
				try
				{
					ip = Encoding.UTF8.GetString(packet, 8, 64).TrimEnd(default(char));
				}
				catch (Exception exception)
				{
					await _audioLogger.DebugAsync("Malformed Packet", exception).ConfigureAwait(continueOnCapturedContext: false);
					return;
				}
				await _audioLogger.DebugAsync("Received Discovery").ConfigureAwait(continueOnCapturedContext: false);
				await ApiClient.SendSelectProtocol(ip).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				if (_connection.State != ConnectionState.Connected)
				{
					return;
				}
				uint ssrc;
				ulong value;
				StreamPair value2;
				if (packet.Length == 8)
				{
					await _audioLogger.DebugAsync("Received Keepalive").ConfigureAwait(continueOnCapturedContext: false);
					ulong num = packet[0] | ((ulong)packet[1] >> 8) | ((ulong)packet[2] >> 16) | ((ulong)packet[3] >> 24) | ((ulong)packet[4] >> 32) | ((ulong)packet[5] >> 40) | ((ulong)packet[6] >> 48) | ((ulong)packet[7] >> 56);
					KeyValuePair<ulong, int> result;
					while (_keepaliveTimes.TryDequeue(out result))
					{
						if (result.Key == num)
						{
							int num2 = Environment.TickCount - result.Value;
							int udpLatency = UdpLatency;
							UdpLatency = num2;
							await _udpLatencyUpdatedEvent.InvokeAsync(udpLatency, num2).ConfigureAwait(continueOnCapturedContext: false);
							break;
						}
					}
				}
				else if (!RTPReadStream.TryReadSsrc(packet, 0, out ssrc))
				{
					await _audioLogger.DebugAsync("Malformed Frame").ConfigureAwait(continueOnCapturedContext: false);
				}
				else if (!_ssrcMap.TryGetValue(ssrc, out value))
				{
					await _audioLogger.DebugAsync($"Unknown SSRC {ssrc}").ConfigureAwait(continueOnCapturedContext: false);
				}
				else if (!_streams.TryGetValue(value, out value2))
				{
					await _audioLogger.DebugAsync($"Unknown User {value}").ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					try
					{
						await value2.Writer.WriteAsync(packet, 0, packet.Length).ConfigureAwait(continueOnCapturedContext: false);
					}
					catch (Exception exception2)
					{
						await _audioLogger.DebugAsync("Malformed Frame", exception2).ConfigureAwait(continueOnCapturedContext: false);
					}
				}
				return;
			}
		}
		catch (Exception exception3)
		{
			await _audioLogger.WarningAsync("Failed to process UDP packet", exception3).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task RunHeartbeatAsync(int intervalMillis, CancellationToken cancelToken)
	{
		int delayInterval = (int)((double)intervalMillis * 0.9);
		try
		{
			await _audioLogger.DebugAsync("Heartbeat Started").ConfigureAwait(continueOnCapturedContext: false);
			while (!cancelToken.IsCancellationRequested)
			{
				int tickCount = Environment.TickCount;
				if (_heartbeatTimes.Count != 0 && tickCount - _lastMessageTime > intervalMillis && ConnectionState == ConnectionState.Connected)
				{
					_connection.Error(new WebSocketException(WebSocketError.InvalidState, "Server missed last heartbeat"));
					return;
				}
				_heartbeatTimes.Enqueue(tickCount);
				try
				{
					await ApiClient.SendHeartbeatAsync().ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception exception)
				{
					await _audioLogger.WarningAsync("Failed to send heartbeat", exception).ConfigureAwait(continueOnCapturedContext: false);
				}
				await Task.Delay(Math.Max(0, delayInterval - Latency), cancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await _audioLogger.DebugAsync("Heartbeat Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await _audioLogger.DebugAsync("Heartbeat Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception2)
		{
			await _audioLogger.ErrorAsync("Heartbeat Errored", exception2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private async Task RunKeepaliveAsync(CancellationToken cancelToken)
	{
		try
		{
			await _audioLogger.DebugAsync("Keepalive Started").ConfigureAwait(continueOnCapturedContext: false);
			while (!cancelToken.IsCancellationRequested)
			{
				int now = Environment.TickCount;
				try
				{
					ulong key = await ApiClient.SendKeepaliveAsync().ConfigureAwait(continueOnCapturedContext: false);
					if (_keepaliveTimes.Count < 12)
					{
						_keepaliveTimes.Enqueue(new KeyValuePair<ulong, int>(key, now));
					}
				}
				catch (Exception exception)
				{
					await _audioLogger.WarningAsync("Failed to send keepalive", exception).ConfigureAwait(continueOnCapturedContext: false);
				}
				await Task.Delay(KeepAliveIntervalMs, cancelToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await _audioLogger.DebugAsync("Keepalive Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			await _audioLogger.DebugAsync("Keepalive Stopped").ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception exception2)
		{
			await _audioLogger.ErrorAsync("Keepalive Errored", exception2).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	public async Task SetSpeakingAsync(bool value)
	{
		if (_isSpeaking != value)
		{
			_isSpeaking = value;
			await ApiClient.SendSetSpeaking(value).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal async Task WaitForDisconnectAsync(TimeSpan timeout)
	{
		TaskCompletionSource<Exception> completion;
		CancellationTokenSource cts;
		if (ConnectionState != ConnectionState.Disconnected)
		{
			completion = new TaskCompletionSource<Exception>();
			cts = new CancellationTokenSource();
			Task.Delay(timeout, cts.Token).ContinueWith(delegate
			{
				completion.TrySetException(new TimeoutException("Exceeded maximum time to wait"));
				cts.Dispose();
			}, cts.Token);
			_connection.Disconnected += HandleDisconnectSubscription;
			await completion.Task.ConfigureAwait(continueOnCapturedContext: false);
		}
		Task HandleDisconnectSubscription(Exception exception, bool reconnect)
		{
			try
			{
				cts.Cancel();
				completion.TrySetResult(exception);
			}
			finally
			{
				_connection.Disconnected -= HandleDisconnectSubscription;
				cts.Dispose();
			}
			return Task.CompletedTask;
		}
	}

	internal void Dispose(bool disposing)
	{
		if (disposing)
		{
			StopAsync().GetAwaiter().GetResult();
			ApiClient.Dispose();
			_stateLock?.Dispose();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
