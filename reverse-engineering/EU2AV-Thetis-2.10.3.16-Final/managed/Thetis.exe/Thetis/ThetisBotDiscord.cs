using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Thetis;

public static class ThetisBotDiscord
{
	public delegate void NewMessageArrived(MessageInfo msg);

	public delegate void Connected();

	public delegate void Disconnected();

	public delegate void Ready();

	public delegate void MessageRemoved(MessageInfo msg);

	private class BotConfig
	{
		[JsonProperty("ID")]
		public string id { get; set; }

		[JsonProperty("Channels")]
		public List<ChannelInfo> channels { get; set; }
	}

	private class ChannelInfo
	{
		[JsonProperty("DiscordChannel")]
		public string discord_channel { get; set; }

		[JsonProperty("CanReceive")]
		public bool can_receive { get; set; }

		[JsonProperty("CanSend")]
		public bool can_send { get; set; }
	}

	public class MessageInfo
	{
		public string Message { get; set; }

		public string Author { get; set; }

		public string ChannelName { get; set; }

		public ulong ChannelID { get; set; }

		public ulong MessageID { get; set; }

		public DateTime Timestamp { get; set; }
	}

	private const string DISCORD_URL = "https://raw.githubusercontent.com/ramdor/Thetis/refs/heads/master/discord.json";

	private const int KEEP_MESSAGES = 10;

	public static NewMessageArrived NewMessageArrivedHandlers;

	public static Connected ConnectedHandlers;

	public static Disconnected DisconnectedHandlers;

	public static Ready ReadyHandlers;

	public static MessageRemoved MessageRemovedHandlers;

	private static DiscordSocketClient _discord_client;

	private static Timer _reconnect_timer;

	private static Timer _channel_info_timer_from_github;

	private static int _retry_attempts;

	private static readonly object _retry_lock;

	private static BotConfig _bot_config;

	private static readonly object _bot_config_lock;

	private static Dictionary<ulong, List<MessageInfo>> _channelMessages;

	private static Timer _messageCleanupTimer;

	private static Timer _ready_timeout_timer;

	private static Timer _queue_process;

	private static List<IMessage> _receive_queue;

	private static bool _process_queue;

	private static List<ulong> _sent_message_ids;

	private static bool _started;

	private static bool _ready;

	private static string _callsign;

	private static string _unique_ids;

	private static string _filter;

	private static string _ignore;

	private static bool _include_time_stamp;

	private static bool _shutting_down;

	private static DateTime _last_message_time;

	private static readonly Regex _callsignRegex;

	private static readonly Dictionary<string, string> _valid_call_cache;

	public static bool IsConnected => _discord_client.ConnectionState == ConnectionState.Connected;

	public static bool IsReady => _ready;

	public static string UniqueIDs
	{
		get
		{
			return _unique_ids;
		}
		set
		{
			_unique_ids = value;
		}
	}

	public static string Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	public static string Ignore
	{
		get
		{
			return _ignore;
		}
		set
		{
			_ignore = value;
		}
	}

	public static bool IncludeTimeStamp
	{
		get
		{
			return _include_time_stamp;
		}
		set
		{
			_include_time_stamp = value;
		}
	}

	static ThetisBotDiscord()
	{
		_retry_attempts = 0;
		_retry_lock = new object();
		_bot_config_lock = new object();
		_channelMessages = new Dictionary<ulong, List<MessageInfo>>();
		_callsignRegex = new Regex("^(2A[A-Z]?|2B[A-Z]?|2C[A-Z]?|2D[A-Z]?|2E[A-Z]?|2F[A-Z]?|2G[A-Z]?|2H[A-Z]?|2I[A-Z]?|2J[A-Z]?|2K[A-Z]?|2L[A-Z]?|2M[A-Z]?|2N[A-Z]?|2O[A-Z]?|2P[A-Z]?|2Q[A-Z]?|2R[A-Z]?|2S[A-Z]?|2T[A-Z]?|2U[A-Z]?|2V[A-Z]?|2W[A-Z]?|2X[A-Z]?|2Y[A-Z]?|2Z[A-Z]?|3A[A-Z]?|3B[A-Z]?|3C[A-Z]?|3D[A-M]|3D[N-Z]|3E[A-Z]?|3F[A-Z]?|3G[A-Z]?|3H[A-Z]?|3I[A-Z]?|3J[A-Z]?|3K[A-Z]?|3L[A-Z]?|3M[A-Z]?|3N[A-Z]?|3O[A-Z]?|3P[A-Z]?|3Q[A-Z]?|3R[A-Z]?|3S[A-Z]?|3T[A-Z]?|3U[A-Z]?|3V[A-Z]?|3W[A-Z]?|3X[A-Z]?|3Y[A-Z]?|3Z[A-Z]?|4A[A-Z]?|4B[A-Z]?|4C[A-Z]?|4D[A-Z]?|4E[A-Z]?|4F[A-Z]?|4G[A-Z]?|4H[A-Z]?|4I[A-Z]?|4J[A-Z]?|4K[A-Z]?|4L[A-Z]?|4M[A-Z]?|4O[A-Z]?|4P[A-Z]?|4Q[A-Z]?|4R[A-Z]?|4S[A-Z]?|4T[A-Z]?|4U[A-Z]?|4V[A-Z]?|4W[A-Z]?|4X[A-Z]?|4Y[A-Z]?|4Z[A-Z]?|5A[A-Z]?|5B[A-Z]?|5C[A-Z]?|5D[A-Z]?|5E[A-Z]?|5F[A-Z]?|5G[A-Z]?|5H[A-Z]?|5I[A-Z]?|5J[A-Z]?|5K[A-Z]?|5L[A-Z]?|5M[A-Z]?|5N[A-Z]?|5O[A-Z]?|5P[A-Z]?|5Q[A-Z]?|5R[A-Z]?|5S[A-Z]?|5T[A-Z]?|5U[A-Z]?|5V[A-Z]?|5W[A-Z]?|5X[A-Z]?|5Y[A-Z]?|5Z[A-Z]?|6A[A-Z]?|6B[A-Z]?|6C[A-Z]?|6D[A-Z]?|6E[A-Z]?|6F[A-Z]?|6G[A-Z]?|6H[A-Z]?|6I[A-Z]?|6J[A-Z]?|6K[A-Z]?|6L[A-Z]?|6M[A-Z]?|6N[A-Z]?|6O[A-Z]?|6P[A-Z]?|6Q[A-Z]?|6R[A-Z]?|6S[A-Z]?|6T[A-Z]?|6U[A-Z]?|6V[A-Z]?|6W[A-Z]?|6X[A-Z]?|6Y[A-Z]?|6Z[A-Z]?|7A[A-Z]?|7B[A-Z]?|7C[A-Z]?|7D[A-Z]?|7E[A-Z]?|7F[A-Z]?|7G[A-Z]?|7H[A-Z]?|7I[A-Z]?|7J[A-Z]?|7K[A-Z]?|7L[A-Z]?|7M[A-Z]?|7N[A-Z]?|7O[A-Z]?|7P[A-Z]?|7Q[A-Z]?|7R[A-Z]?|7S[A-Z]?|7T[A-Z]?|7U[A-Z]?|7V[A-Z]?|7W[A-Z]?|7X[A-Z]?|7Y[A-Z]?|7Z[A-Z]?|8A[A-Z]?|8B[A-Z]?|8C[A-Z]?|8D[A-Z]?|8E[A-Z]?|8F[A-Z]?|8G[A-Z]?|8H[A-Z]?|8I[A-Z]?|8J[A-Z]?|8K[A-Z]?|8L[A-Z]?|8M[A-Z]?|8N[A-Z]?|8O[A-Z]?|8P[A-Z]?|8Q[A-Z]?|8R[A-Z]?|8S[A-Z]?|8T[A-Z]?|8U[A-Z]?|8V[A-Z]?|8W[A-Z]?|8X[A-Z]?|8Y[A-Z]?|8Z[A-Z]?|9A[A-Z]?|9B[A-Z]?|9C[A-Z]?|9D[A-Z]?|9E[A-Z]?|9F[A-Z]?|9G[A-Z]?|9H[A-Z]?|9I[A-Z]?|9J[A-Z]?|9K[A-Z]?|9L[A-Z]?|9M[A-Z]?|9N[A-Z]?|9O[A-Z]?|9P[A-Z]?|9Q[A-Z]?|9R[A-Z]?|9S[A-Z]?|9T[A-Z]?|9U[A-Z]?|9V[A-Z]?|9W[A-Z]?|9X[A-Z]?|9Y[A-Z]?|9Z[A-Z]?|A2[A-Z]?|A3[A-Z]?|A4[A-Z]?|A5[A-Z]?|A6[A-Z]?|A7[A-Z]?|A8[A-Z]?|A9[A-Z]?|AA[A-Z]?|AB[A-Z]?|AC[A-Z]?|AD[A-Z]?|AE[A-Z]?|AF[A-Z]?|AG[A-Z]?|AH[A-Z]?|AI[A-Z]?|AJ[A-Z]?|AK[A-Z]?|AL[A-Z]?|AM[A-Z]?|AN[A-Z]?|AO[A-Z]?|AP[A-Z]?|AQ[A-Z]?|AR[A-Z]?|AS[A-Z]?|AT[A-Z]?|AU[A-Z]?|AV[A-Z]?|AW[A-Z]?|AX[A-Z]?|AY[A-Z]?|AZ[A-Z]?|B[A-Z]{0,2}|C2[A-Z]?|C3[A-Z]?|C4[A-Z]?|C5[A-Z]?|C6[A-Z]?|C7[A-Z]?|C8[A-Z]?|C9[A-Z]?|CA[A-Z]?|CB[A-Z]?|CC[A-Z]?|CD[A-Z]?|CE[A-Z]?|CF[A-Z]?|CG[A-Z]?|CH[A-Z]?|CI[A-Z]?|CJ[A-Z]?|CK[A-Z]?|CL[A-Z]?|CM[A-Z]?|CN[A-Z]?|CO[A-Z]?|CP[A-Z]?|CQ[A-Z]?|CR[A-Z]?|CS[A-Z]?|CT[A-Z]?|CU[A-Z]?|CV[A-Z]?|CW[A-Z]?|CX[A-Z]?|CY[A-Z]?|CZ[A-Z]?|D2[A-Z]?|D3[A-Z]?|D4[A-Z]?|D5[A-Z]?|D6[A-Z]?|D7[A-Z]?|D8[A-Z]?|D9[A-Z]?|DA[A-Z]?|DB[A-Z]?|DC[A-Z]?|DD[A-Z]?|DE[A-Z]?|DF[A-Z]?|DG[A-Z]?|DH[A-Z]?|DI[A-Z]?|DJ[A-Z]?|DK[A-Z]?|DL[A-Z]?|DM[A-Z]?|DN[A-Z]?|DO[A-Z]?|DP[A-Z]?|DQ[A-Z]?|DR[A-Z]?|DS[A-Z]?|DT[A-Z]?|DU[A-Z]?|DV[A-Z]?|DW[A-Z]?|DX[A-Z]?|DY[A-Z]?|DZ[A-Z]?|E2[A-Z]?|E3[A-Z]?|E4[A-Z]?|E5[A-Z]?|E6[A-Z]?|E7[A-Z]?|EA[A-Z]?|EB[A-Z]?|EC[A-Z]?|ED[A-Z]?|EE[A-Z]?|EF[A-Z]?|EG[A-Z]?|EH[A-Z]?|EI[A-Z]?|EJ[A-Z]?|EK[A-Z]?|EL[A-Z]?|EM[A-Z]?|EN[A-Z]?|EO[A-Z]?|EP[A-Z]?|EQ[A-Z]?|ER[A-Z]?|ES[A-Z]?|ET[A-Z]?|EU[A-Z]?|EV[A-Z]?|EW[A-Z]?|EX[A-Z]?|EY[A-Z]?|EZ[A-Z]?|F[A-Z]{0,2}|G[A-Z]{0,2}|H2[A-Z]?|H3[A-Z]?|H4[A-Z]?|H6[A-Z]?|H7[A-Z]?|H8[A-Z]?|H9[A-Z]?|HA[A-Z]?|HB[A-Z]?|HC[A-Z]?|HD[A-Z]?|HE[A-Z]?|HF[A-Z]?|HG[A-Z]?|HH[A-Z]?|HI[A-Z]?|HJ[A-Z]?|HK[A-Z]?|HL[A-Z]?|HM[A-Z]?|HN[A-Z]?|HO[A-Z]?|HP[A-Z]?|HQ[A-Z]?|HR[A-Z]?|HS[A-Z]?|HT[A-Z]?|HU[A-Z]?|HV[A-Z]?|HW[A-Z]?|HX[A-Z]?|HY[A-Z]?|HZ[A-Z]?|I[A-Z]{0,2}|J2[A-Z]?|J3[A-Z]?|J4[A-Z]?|J5[A-Z]?|J6[A-Z]?|J7[A-Z]?|J8[A-Z]?|JA[A-Z]?|JB[A-Z]?|JC[A-Z]?|JD[A-Z]?|JE[A-Z]?|JF[A-Z]?|JG[A-Z]?|JH[A-Z]?|JI[A-Z]?|JJ[A-Z]?|JK[A-Z]?|JL[A-Z]?|JM[A-Z]?|JN[A-Z]?|JO[A-Z]?|JP[A-Z]?|JQ[A-Z]?|JR[A-Z]?|JS[A-Z]?|JT[A-Z]?|JU[A-Z]?|JV[A-Z]?|JW[A-Z]?|JX[A-Z]?|JY[A-Z]?|JZ[A-Z]?|K[A-Z]{0,2}|L2[A-Z]?|L3[A-Z]?|L4[A-Z]?|L5[A-Z]?|L6[A-Z]?|L7[A-Z]?|L8[A-Z]?|L9[A-Z]?|LA[A-Z]?|LB[A-Z]?|LC[A-Z]?|LD[A-Z]?|LE[A-Z]?|LF[A-Z]?|LG[A-Z]?|LH[A-Z]?|LI[A-Z]?|LJ[A-Z]?|LK[A-Z]?|LL[A-Z]?|LM[A-Z]?|LN[A-Z]?|LO[A-Z]?|LP[A-Z]?|LQ[A-Z]?|LR[A-Z]?|LS[A-Z]?|LT[A-Z]?|LU[A-Z]?|LV[A-Z]?|LW[A-Z]?|LX[A-Z]?|LY[A-Z]?|LZ[A-Z]?|M[A-Z]{0,2}|N[A-Z]{0,2}|OA[A-Z]?|OB[A-Z]?|OC[A-Z]?|OD[A-Z]?|OE[A-Z]?|OF[A-Z]?|OG[A-Z]?|OH[A-Z]?|OI[A-Z]?|OJ[A-Z]?|OK[A-Z]?|OL[A-Z]?|OM[A-Z]?|ON[A-Z]?|OO[A-Z]?|OP[A-Z]?|OQ[A-Z]?|OR[A-Z]?|OS[A-Z]?|OT[A-Z]?|OU[A-Z]?|OV[A-Z]?|OW[A-Z]?|OX[A-Z]?|OY[A-Z]?|OZ[A-Z]?|P2[A-Z]?|P3[A-Z]?|P4[A-Z]?|P5[A-Z]?|P6[A-Z]?|P7[A-Z]?|P8[A-Z]?|P9[A-Z]?|PA[A-Z]?|PB[A-Z]?|PC[A-Z]?|PD[A-Z]?|PE[A-Z]?|PF[A-Z]?|PG[A-Z]?|PH[A-Z]?|PI[A-Z]?|PJ[A-Z]?|PK[A-Z]?|PL[A-Z]?|PM[A-Z]?|PN[A-Z]?|PO[A-Z]?|PP[A-Z]?|PQ[A-Z]?|PR[A-Z]?|PS[A-Z]?|PT[A-Z]?|PU[A-Z]?|PV[A-Z]?|PW[A-Z]?|PX[A-Z]?|PY[A-Z]?|PZ[A-Z]?|R[A-Z]{0,2}|S2[A-Z]?|S3[A-Z]?|S5[A-Z]?|S6[A-Z]?|S7[A-Z]?|S8[A-Z]?|S9[A-Z]?|SA[A-Z]?|SB[A-Z]?|SC[A-Z]?|SD[A-Z]?|SE[A-Z]?|SF[A-Z]?|SG[A-Z]?|SH[A-Z]?|SI[A-Z]?|SJ[A-Z]?|SK[A-Z]?|SL[A-Z]?|SM[A-Z]?|SN[A-Z]?|SO[A-Z]?|SP[A-Z]?|SQ[A-Z]?|SR[A-Z]?|SS[A-M]|SS[N-Z]|ST[A-Z]?|SU[A-Z]?|SV[A-Z]?|SW[A-Z]?|SX[A-Z]?|SY[A-Z]?|SZ[A-Z]?|T2[A-Z]?|T3[A-Z]?|T4[A-Z]?|T5[A-Z]?|T6[A-Z]?|T7[A-Z]?|T8[A-Z]?|TA[A-Z]?|TB[A-Z]?|TC[A-Z]?|TD[A-Z]?|TE[A-Z]?|TF[A-Z]?|TG[A-Z]?|TH[A-Z]?|TI[A-Z]?|TJ[A-Z]?|TK[A-Z]?|TL[A-Z]?|TM[A-Z]?|TN[A-Z]?|TO[A-Z]?|TP[A-Z]?|TQ[A-Z]?|TR[A-Z]?|TS[A-Z]?|TT[A-Z]?|TU[A-Z]?|TV[A-Z]?|TW[A-Z]?|TX[A-Z]?|TY[A-Z]?|TZ[A-Z]?|UA[A-Z]?|UB[A-Z]?|UC[A-Z]?|UD[A-Z]?|UE[A-Z]?|UF[A-Z]?|UG[A-Z]?|UH[A-Z]?|UI[A-Z]?|UJ[A-Z]?|UK[A-Z]?|UL[A-Z]?|UM[A-Z]?|UN[A-Z]?|UO[A-Z]?|UP[A-Z]?|UQ[A-Z]?|UR[A-Z]?|US[A-Z]?|UT[A-Z]?|UU[A-Z]?|UV[A-Z]?|UW[A-Z]?|UX[A-Z]?|UY[A-Z]?|UZ[A-Z]?|V2[A-Z]?|V3[A-Z]?|V4[A-Z]?|V5[A-Z]?|V6[A-Z]?|V7[A-Z]?|V8[A-Z]?|VA[A-Z]?|VB[A-Z]?|VC[A-Z]?|VD[A-Z]?|VE[A-Z]?|VF[A-Z]?|VG[A-Z]?|VH[A-Z]?|VI[A-Z]?|VJ[A-Z]?|VK[A-Z]?|VL[A-Z]?|VM[A-Z]?|VN[A-Z]?|VO[A-Z]?|VP[A-Z]?|VQ[A-Z]?|VR[A-Z]?|VS[A-Z]?|VT[A-Z]?|VU[A-Z]?|VV[A-Z]?|VW[A-Z]?|VX[A-Z]?|VY[A-Z]?|VZ[A-Z]?|W[A-Z]{0,2}|XA[A-Z]?|XB[A-Z]?|XC[A-Z]?|XD[A-Z]?|XE[A-Z]?|XF[A-Z]?|XG[A-Z]?|XH[A-Z]?|XI[A-Z]?|XJ[A-Z]?|XK[A-Z]?|XL[A-Z]?|XM[A-Z]?|XN[A-Z]?|XO[A-Z]?|XP[A-Z]?|XQ[A-Z]?|XR[A-Z]?|XS[A-Z]?|XT[A-Z]?|XU[A-Z]?|XV[A-Z]?|XW[A-Z]?|XX[A-Z]?|XY[A-Z]?|XZ[A-Z]?|Y2[A-Z]?|Y3[A-Z]?|Y4[A-Z]?|Y5[A-Z]?|Y6[A-Z]?|Y7[A-Z]?|Y8[A-Z]?|Y9[A-Z]?|YA[A-Z]?|YB[A-Z]?|YC[A-Z]?|YD[A-Z]?|YE[A-Z]?|YF[A-Z]?|YG[A-Z]?|YH[A-Z]?|YI[A-Z]?|YJ[A-Z]?|YK[A-Z]?|YL[A-Z]?|YM[A-Z]?|YN[A-Z]?|YO[A-Z]?|YP[A-Z]?|YQ[A-Z]?|YR[A-Z]?|YS[A-Z]?|YT[A-Z]?|YU[A-Z]?|YV[A-Z]?|YW[A-Z]?|YX[A-Z]?|YY[A-Z]?|Z2[A-Z]?|Z3[A-Z]?|Z8[A-Z]?|ZA[A-Z]?|ZB[A-Z]?|ZC[A-Z]?|ZD[A-Z]?|ZE[A-Z]?|ZF[A-Z]?|ZG[A-Z]?|ZH[A-Z]?|ZI[A-Z]?|ZJ[A-Z]?|ZK[A-Z]?|ZL[A-Z]?|ZM[A-Z]?|ZN[A-Z]?|ZO[A-Z]?|ZP[A-Z]?|ZQ[A-Z]?|ZR[A-Z]?|ZS[A-Z]?|ZT[A-Z]?|ZU[A-Z]?|ZV[A-Z]?|ZW[A-Z]?|ZX[A-Z]?|ZY[A-Z]?|ZZ[A-Z]?)[0-9][0-9A-Z]{0,3}[A-Z]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		_valid_call_cache = new Dictionary<string, string>();
		_shutting_down = false;
		_last_message_time = DateTime.MinValue;
		_started = false;
		_ready = false;
		_callsign = "";
		_unique_ids = "";
		_filter = "";
		_ignore = "";
		_include_time_stamp = true;
		_process_queue = true;
		_receive_queue = new List<IMessage>();
		_sent_message_ids = new List<ulong>();
		_bot_config = new BotConfig();
		_discord_client = new DiscordSocketClient(new DiscordSocketConfig
		{
			GatewayIntents = (GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent)
		});
		_discord_client.MessageReceived += messageReceived;
		_discord_client.Disconnected += OnDisconnected;
		_discord_client.Connected += OnConnected;
		_discord_client.Ready += OnReady;
		_discord_client.MessageDeleted += OnMessageDeleted;
	}

	private static void cleanupOldMessages()
	{
		DateTime now = DateTime.UtcNow;
		lock (_channelMessages)
		{
			List<MessageInfo> list = new List<MessageInfo>();
			foreach (KeyValuePair<ulong, List<MessageInfo>> item in _channelMessages.ToList())
			{
				if (item.Value.Count <= 10)
				{
					continue;
				}
				List<MessageInfo> list2 = (from m in item.Value
					orderby m.Timestamp
					where (now - m.Timestamp).TotalMinutes > 5.0
					select m).ToList();
				if (list2.Count <= 0)
				{
					continue;
				}
				int count = item.Value.Count - 10;
				foreach (MessageInfo item2 in list2.Take(count))
				{
					item.Value.Remove(item2);
					list.Add(item2);
				}
			}
			foreach (MessageInfo item3 in list)
			{
				if (!_shutting_down && MessageRemovedHandlers != null)
				{
					Delegate[] invocationList = MessageRemovedHandlers.GetInvocationList();
					for (int num = 0; num < invocationList.Length; num++)
					{
						((MessageRemoved)invocationList[num]).BeginInvoke(item3, null, null);
					}
				}
			}
		}
	}

	public static async Task loadChannelInfoFromGitHub()
	{
		bool git_hub_ok = false;
		try
		{
			string value = await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/ramdor/Thetis/refs/heads/master/discord.json");
			lock (_bot_config_lock)
			{
				_bot_config = JsonConvert.DeserializeObject<BotConfig>(value);
				git_hub_ok = true;
			}
			if (_reconnect_timer != null && _discord_client.ConnectionState != ConnectionState.Connected)
			{
				await tryConnect();
			}
			_retry_attempts = 0;
			_channel_info_timer_from_github.Change(TimeSpan.FromMinutes(30.0), TimeSpan.FromMinutes(30.0));
		}
		catch (Exception)
		{
			if (!git_hub_ok)
			{
				adjustRetryInterval();
			}
		}
	}

	private static void adjustRetryInterval()
	{
		_retry_attempts++;
		if (_retry_attempts > 21)
		{
			_retry_attempts = 21;
		}
		if (_retry_attempts <= 5)
		{
			_channel_info_timer_from_github.Change(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0));
		}
		else if (_retry_attempts <= 10)
		{
			_channel_info_timer_from_github.Change(TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0));
		}
		else if (_retry_attempts <= 15)
		{
			_channel_info_timer_from_github.Change(TimeSpan.FromMinutes(5.0), TimeSpan.FromMinutes(5.0));
		}
		else if (_retry_attempts <= 20)
		{
			_channel_info_timer_from_github.Change(TimeSpan.FromMinutes(10.0), TimeSpan.FromMinutes(10.0));
		}
		else
		{
			_channel_info_timer_from_github.Change(TimeSpan.FromMinutes(30.0), TimeSpan.FromMinutes(30.0));
		}
	}

	public static void ConnectStart()
	{
		if (_started || _shutting_down)
		{
			return;
		}
		_started = true;
		_ready = false;
		_queue_process = new Timer(delegate
		{
			processQueue();
		}, null, TimeSpan.Zero, TimeSpan.FromSeconds(1.0));
		_messageCleanupTimer = new Timer(delegate
		{
			cleanupOldMessages();
		}, null, TimeSpan.Zero, TimeSpan.FromSeconds(30.0));
		_channel_info_timer_from_github = new Timer(async delegate
		{
			await loadChannelInfoFromGitHub();
		}, null, TimeSpan.Zero, TimeSpan.FromMinutes(30.0));
		_reconnect_timer = new Timer(async delegate
		{
			if (_discord_client.ConnectionState == ConnectionState.Disconnected)
			{
				await tryConnect();
			}
		}, null, TimeSpan.Zero, TimeSpan.FromMinutes(1.0));
	}

	public static void ConnectStop()
	{
		if (!_started)
		{
			return;
		}
		_queue_process?.Dispose();
		_messageCleanupTimer?.Dispose();
		_channel_info_timer_from_github?.Dispose();
		_reconnect_timer?.Dispose();
		for (int num = 10; num > 0; num--)
		{
			Task.Run(async delegate
			{
				await _discord_client.LogoutAsync();
				await _discord_client.StopAsync();
			}).GetAwaiter().GetResult();
			if (_discord_client.ConnectionState == ConnectionState.Disconnected)
			{
				break;
			}
			Thread.Sleep(100);
		}
		_ready = false;
		_started = false;
		if (!_shutting_down && DisconnectedHandlers != null)
		{
			Delegate[] invocationList = DisconnectedHandlers.GetInvocationList();
			for (int num2 = 0; num2 < invocationList.Length; num2++)
			{
				((Disconnected)invocationList[num2]).BeginInvoke(null, null);
			}
		}
	}

	public static void Shutdown()
	{
		_shutting_down = true;
		ConnectStop();
		_discord_client.Dispose();
	}

	private static async Task tryConnect()
	{
		_ = 1;
		try
		{
			_ready = false;
			string text = "";
			lock (_bot_config_lock)
			{
				text = _bot_config.id;
			}
			if (!string.IsNullOrEmpty(text))
			{
				text = Common.DeserializeFromBase64<string>(text);
				await _discord_client.LoginAsync(TokenType.Bot, text);
				await _discord_client.StartAsync();
			}
		}
		catch (Exception)
		{
		}
	}

	private static Task OnReady()
	{
		_ready_timeout_timer?.Dispose();
		_ready = true;
		getLastMessages(10).Wait();
		_ready = true;
		if (!_shutting_down && ReadyHandlers != null)
		{
			Delegate[] invocationList = ReadyHandlers.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				((Ready)invocationList[i]).BeginInvoke(null, null);
			}
		}
		return Task.CompletedTask;
	}

	private static Task OnConnected()
	{
		_ready_timeout_timer?.Dispose();
		_ready_timeout_timer = new Timer(async delegate
		{
			if (!_ready)
			{
				await _discord_client.StopAsync();
			}
		}, null, TimeSpan.FromSeconds(20.0), Timeout.InfiniteTimeSpan);
		if (!_shutting_down && ConnectedHandlers != null)
		{
			Delegate[] invocationList = ConnectedHandlers.GetInvocationList();
			for (int num = 0; num < invocationList.Length; num++)
			{
				((Connected)invocationList[num]).BeginInvoke(null, null);
			}
		}
		return Task.CompletedTask;
	}

	private static Task OnDisconnected(Exception exception)
	{
		_ready = false;
		if (!_shutting_down && DisconnectedHandlers != null)
		{
			Delegate[] invocationList = DisconnectedHandlers.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				((Disconnected)invocationList[i]).BeginInvoke(null, null);
			}
		}
		return Task.CompletedTask;
	}

	private static Task messageReceived(SocketMessage message)
	{
		lock (_receive_queue)
		{
			if (!_shutting_down && !_sent_message_ids.Contains(message.Id))
			{
				_receive_queue.Add(message);
			}
		}
		return Task.CompletedTask;
	}

	private static void processQueue()
	{
		if (!_process_queue)
		{
			return;
		}
		lock (_receive_queue)
		{
			foreach (IMessage item in _receive_queue)
			{
				if (!_sent_message_ids.Contains(item.Id))
				{
					handleQueuedMessage(item);
				}
			}
			_receive_queue.Clear();
		}
	}

	private static void handleQueuedMessage(IMessage message)
	{
		if ((!(message is SocketUserMessage) && !(message is RestUserMessage)) || message.Attachments.Any())
		{
			return;
		}
		bool flag = false;
		lock (_bot_config_lock)
		{
			foreach (ChannelInfo item in _bot_config.channels.Where((ChannelInfo c) => c.can_receive).ToList())
			{
				if (!string.IsNullOrEmpty(item.discord_channel) && ulong.TryParse(item.discord_channel, out var result) && result == message.Channel.Id)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		string text = message.CleanContent.Replace("\r\n", " ").Replace("\n", " ").Trim();
		if (!string.IsNullOrEmpty(_ignore))
		{
			bool flag2 = false;
			string[] array = _ignore.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				string value = array[num].Trim();
				if (text.Contains(value))
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				return;
			}
		}
		string text2 = "";
		int num2 = text.IndexOf("  [");
		if (num2 >= 0)
		{
			int num3 = num2 + 3;
			int num4 = text.IndexOf("]", num3);
			if (num4 >= 0)
			{
				text2 = text.Substring(num3, num4 - num3);
				text = text.Substring(0, num2);
			}
		}
		if (!string.IsNullOrEmpty(_filter))
		{
			bool flag3 = false;
			string[] array = _filter.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				string text3 = array[num].Trim();
				if (text.Contains(text3 + " " + text2))
				{
					flag3 = true;
				}
			}
			if (!flag3)
			{
				return;
			}
		}
		string authorName = getAuthorName(message);
		string message2 = text;
		ulong id = message.Channel.Id;
		MessageInfo messageInfo = new MessageInfo
		{
			Message = message2,
			Author = authorName,
			ChannelID = id,
			MessageID = message.Id,
			Timestamp = DateTime.UtcNow
		};
		lock (_channelMessages)
		{
			if (!_channelMessages.ContainsKey(id))
			{
				_channelMessages[id] = new List<MessageInfo>();
			}
			_channelMessages[id].Insert(0, messageInfo);
		}
		if (!_shutting_down && NewMessageArrivedHandlers != null)
		{
			Delegate[] invocationList = NewMessageArrivedHandlers.GetInvocationList();
			for (int num = 0; num < invocationList.Length; num++)
			{
				((NewMessageArrived)invocationList[num]).BeginInvoke(messageInfo, null, null);
			}
		}
	}

	private static string getAuthorName(IMessage message)
	{
		string text = null;
		if (message.Channel is SocketGuildChannel { Guild: var guild })
		{
			SocketGuildUser user = guild.GetUser(message.Author.Id);
			if (user == null)
			{
				try
				{
					RestGuildUser result = _discord_client.Rest.GetGuildUserAsync(guild.Id, message.Author.Id).Result;
					if (result != null)
					{
						text = result.Nickname ?? result.DisplayName ?? result.GlobalName;
					}
				}
				catch (Exception)
				{
				}
			}
			else
			{
				text = user.Nickname ?? user.DisplayName ?? user.GlobalName;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Trim();
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return message.Author.Username ?? "(unknown user)";
	}

	public static async Task SendMessage(string message, ulong channel_id = 0uL)
	{
		if (_discord_client.ConnectionState == ConnectionState.Disconnected || (DateTime.UtcNow - _last_message_time).TotalSeconds < 5.0)
		{
			return;
		}
		_last_message_time = DateTime.UtcNow;
		message = message.Replace("\r\n", " ").Replace("\n", " ").Trim();
		ChannelInfo channelInfo;
		lock (_bot_config_lock)
		{
			channelInfo = _bot_config.channels.FirstOrDefault((ChannelInfo c) => c.can_send && (channel_id == 0L || (ulong.TryParse(c.discord_channel, out var result2) && result2 == channel_id)));
		}
		if (channelInfo == null || !ulong.TryParse(channelInfo.discord_channel, out var result) || !(_discord_client.GetChannel(result) is IMessageChannel messageChannel))
		{
			return;
		}
		try
		{
			string text = (_include_time_stamp ? (_last_message_time.ToString("HH:mm") + "z - ") : "") + _callsign + " - " + message;
			string[] array = _unique_ids.Split(',');
			if (array.Length != 0 && !string.IsNullOrEmpty(array[0]))
			{
				text += "  [";
				string[] array2 = array;
				for (int num = 0; num < array2.Length; num++)
				{
					string text2 = array2[num].Trim();
					text2 = text2.Replace("[", "");
					text2 = text2.Replace("]", "");
					text = text + text2 + ",";
				}
				text = text.Substring(0, text.Length - 1);
				text += "]";
			}
			_process_queue = false;
			IUserMessage userMessage = await messageChannel.SendMessageAsync(text).ConfigureAwait(continueOnCapturedContext: false);
			lock (_receive_queue)
			{
				_sent_message_ids.Add(userMessage.Id);
				_process_queue = true;
				processQueue();
			}
		}
		catch (Exception)
		{
		}
	}

	private static async Task<List<MessageInfo>> getLastMessages(int n)
	{
		List<MessageInfo> messages = new List<MessageInfo>();
		lock (_channelMessages)
		{
			_channelMessages.Clear();
		}
		List<ChannelInfo> list;
		lock (_bot_config_lock)
		{
			list = _bot_config.channels.Where((ChannelInfo c) => c.can_receive).ToList();
		}
		foreach (ChannelInfo item in list)
		{
			if (!ulong.TryParse(item.discord_channel, out var result) || !(_discord_client.GetChannel(result) is SocketTextChannel socketTextChannel))
			{
				continue;
			}
			try
			{
				foreach (IMessage item2 in await socketTextChannel.GetMessagesAsync(n).FlattenAsync())
				{
					lock (_receive_queue)
					{
						_receive_queue.Insert(0, item2);
					}
				}
			}
			catch (Exception)
			{
			}
		}
		return messages;
	}

	public static string GetMessagesString(ulong channel_id, int message = 0, bool include_author = true)
	{
		lock (_channelMessages)
		{
			if (_channelMessages.TryGetValue(channel_id, out var value) && value != null && value.Count > message)
			{
				MessageInfo messageInfo = value[message];
				return include_author ? (messageInfo.Author + " - " + messageInfo.Message) : (messageInfo.Message ?? "");
			}
		}
		return string.Empty;
	}

	public static List<(ulong ChannelID, string ChannelName)> GetReceivableChannels()
	{
		List<(ulong, string)> list = new List<(ulong, string)>();
		lock (_bot_config_lock)
		{
			foreach (ChannelInfo item in _bot_config.channels.Where((ChannelInfo c) => c.can_receive).ToList())
			{
				if (ulong.TryParse(item.discord_channel, out var result))
				{
					list.Add((result, item.discord_channel));
				}
			}
			return list;
		}
	}

	public static List<(ulong ChannelID, string ChannelName)> GetSendableChannels()
	{
		List<(ulong, string)> list = new List<(ulong, string)>();
		lock (_bot_config_lock)
		{
			foreach (ChannelInfo item in _bot_config.channels.Where((ChannelInfo c) => c.can_send).ToList())
			{
				if (ulong.TryParse(item.discord_channel, out var result))
				{
					list.Add((result, item.discord_channel));
				}
			}
			return list;
		}
	}

	private static Task OnMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
	{
		ulong message_id = cachedMessage.Id;
		ulong id = cachedChannel.Id;
		lock (_channelMessages)
		{
			if (_channelMessages.ContainsKey(id))
			{
				MessageInfo messageInfo = _channelMessages[id].FirstOrDefault((MessageInfo m) => m.MessageID == message_id);
				if (messageInfo != null)
				{
					_channelMessages[id].Remove(messageInfo);
					_sent_message_ids.Remove(message_id);
					if (!_shutting_down && MessageRemovedHandlers != null)
					{
						Delegate[] invocationList = MessageRemovedHandlers.GetInvocationList();
						for (int num = 0; num < invocationList.Length; num++)
						{
							((MessageRemoved)invocationList[num]).BeginInvoke(messageInfo, null, null);
						}
					}
				}
			}
		}
		return Task.CompletedTask;
	}

	public static bool IsValidCallsign(string callsign, out string country)
	{
		string key = callsign.ToUpperInvariant();
		if (_valid_call_cache.TryGetValue(key, out country))
		{
			return true;
		}
		country = null;
		if (!_callsignRegex.IsMatch(callsign))
		{
			return false;
		}
		CountryData.PrefixData callsignData = CountryData.GetCallsignData(callsign);
		if (callsignData != null)
		{
			country = callsignData.Country;
		}
		_valid_call_cache[key] = country;
		return true;
	}

	public static void SetEnabled(bool enabled)
	{
		if (enabled && IsValidCallsign(_callsign, out var _))
		{
			ConnectStart();
		}
		else
		{
			ConnectStop();
		}
	}

	public static void SetCallsign(string callsign)
	{
		_callsign = callsign.ToUpper().Left(12);
		if (_started && !IsValidCallsign(_callsign, out var _))
		{
			ConnectStop();
		}
	}
}
