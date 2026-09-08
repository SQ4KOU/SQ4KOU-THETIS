using System.Runtime.InteropServices;

namespace Thetis;

internal static class WaveThing
{
	public delegate void createWplay(int id);

	public delegate void createWrecord(int id);

	public unsafe delegate void WPlay(int state, double* data);

	public unsafe delegate void WRecord(int state, int pos, double* data);

	private static createWplay cwpDel = createWavePlayer;

	private static createWrecord cwrDel = createWaveRecorder;

	private const int nplayers = 16;

	public static PlayWave[] wplayer = new PlayWave[16];

	private static WPlay[] pplay = new WPlay[16];

	public static WaveFileReader1[] wave_file_reader = new WaveFileReader1[16];

	private const int nrecorders = 16;

	public static RecordWave[] wrecorder = new RecordWave[16];

	private static WRecord[] precord = new WRecord[16];

	public static WaveFileWriter[] wave_file_writer = new WaveFileWriter[16];

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBCreateWPlay(createWplay del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetWavePlayerRun(int id, int run);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBCreateWRecord(createWrecord del);

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SetWaveRecorderRun(int id, int run);

	public static void initWaves()
	{
		SendCBCreateWPlay(cwpDel);
		SendCBCreateWRecord(cwrDel);
	}

	[DllImport("ChannelMaster.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBWavePlayer(int id, WPlay del);

	public unsafe static void createWavePlayer(int id)
	{
		wplayer[id] = new PlayWave();
		pplay[id] = wplayer[id].wplay;
		SendCBWavePlayer(id, pplay[id]);
		wplayer[id].ID = id;
	}

	[DllImport("ChannelMaster", CallingConvention = CallingConvention.Cdecl)]
	public static extern void SendCBWaveRecorder(int id, WRecord del);

	public unsafe static void createWaveRecorder(int id)
	{
		wrecorder[id] = new RecordWave();
		precord[id] = wrecorder[id].wrecord;
		SendCBWaveRecorder(id, precord[id]);
		wrecorder[id].ID = id;
	}

	public static void UpdateMox()
	{
		for (int i = 0; i < 16; i++)
		{
			if (wave_file_writer[i] != null)
			{
				wave_file_writer[i].UpdateMox();
			}
		}
	}
}
