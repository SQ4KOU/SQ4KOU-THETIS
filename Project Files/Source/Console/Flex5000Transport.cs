using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Thetis
{
    // Dedicated FLEX-5000 hardware transport.
    // Thetis/ChannelMaster/WDSP remains the DSP/UI engine.
    // FLEX hardware control uses the installed x86 PAL/FWC runtime and
    // sample transport uses cmASIO.dll's dedicated 8x8/192k FlexASIO host.
    internal static unsafe class Flex5000Transport
    {
        private const string FlexRuntimeDir = @"C:\Program Files (x86)\FlexRadio Systems\PowerSDR v2.8.0";
        private const string PalFileName = "pal.dll";
        private const string FlexAsioDriver = "ASIO FlexRadio";
        private const int SampleRate = 192000;
        private const int TxInputRate = 48000;
        private const int TxDecimate = 4;
        private const float TxSafetyScale = 0.02f;
        private const uint LoadWithAlteredSearchPath = 0x00000008;

        // PowerSDR/FLEX-5000 FWC opcodes used by the proven native path.
        private const int OpGetFirmwareRev = 1200;
        private const int OpInitialize = 1219;
        private const int OpReadCodecReg = 1242;
        private const int OpWriteCodecReg = 1243;
        private const int OpSetTrxPreamp = 1247;
        private const int OpSetQse = 1255;
        private const int OpSetRx1Filter = 1257;
        private const int OpSetTxFilter = 1259;
        private const int OpSetPaFilter = 1260;
        private const int OpSetIntSpkr = 1261;
        private const int OpSetHeadphone = 1267;
        private const int OpSetTr = 1276;
        private const int OpSetRx1Ant = 1278;
        private const int OpSetTxAnt = 1279;
        private const int OpSetPaBias = 1285;
        private const int OpSetMox = 1292;
        private const int OpSetStandby = 1320;
        private const int OpSetRx1FreqTw = 1347;
        private const int OpSetTxFreqTw = 1349;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint flags);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr module, string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool PalInitDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PalGetNumDevicesDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool PalGetDeviceInfoDelegate(uint index, out uint model, out uint serial);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool PalSelectDeviceDelegate(uint index);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PalReadOpDelegate(int opcode, uint data1, uint data2, out uint value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PalWriteUIntDelegate(int opcode, uint data1, uint data2);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PalWriteUIntFloatDelegate(int opcode, uint data1, float data2);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int PalWriteFloatUIntDelegate(int opcode, float data1, uint data2);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void PalSetBufferSizeDelegate(uint frames);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FlexAsioCallback(
            IntPtr in0, IntPtr in1, IntPtr in2, IntPtr in3,
            IntPtr in4, IntPtr in5, IntPtr in6, IntPtr in7,
            IntPtr out0, IntPtr out1, IntPtr out2, IntPtr out3,
            IntPtr out4, IntPtr out5, IntPtr out6, IntPtr out7,
            int frames);

        [DllImport("cmASIO.dll", EntryPoint = "prepareFlexASIO", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int PrepareFlexAsio(int sampleRate, string driverName, FlexAsioCallback callback);
        [DllImport("cmASIO.dll", EntryPoint = "flexAsioStart", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FlexAsioStart();
        [DllImport("cmASIO.dll", EntryPoint = "flexAsioStop", CallingConvention = CallingConvention.Cdecl)]
        private static extern int FlexAsioStop();
        [DllImport("cmASIO.dll", EntryPoint = "unloadFlexASIO", CallingConvention = CallingConvention.Cdecl)]
        private static extern void UnloadFlexAsio();
        [DllImport("cmASIO.dll", EntryPoint = "getFlexASIOBlockSize", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFlexAsioBlockSize();
        [DllImport("cmASIO.dll", EntryPoint = "getFlexASIOError", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetFlexAsioError();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CmOutboundDelegate(int id, int nsamples, double* buffer);
        [DllImport("ChannelMaster.dll", EntryPoint = "SendpOutboundRx", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SendpOutboundRx(CmOutboundDelegate callback);
        [DllImport("ChannelMaster.dll", EntryPoint = "SendpOutboundTx", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SendpOutboundTx(CmOutboundDelegate callback);

        private static readonly object Sync = new object();
        private static IntPtr _palModule;
        private static PalInitDelegate _palInit;
        private static PalGetNumDevicesDelegate _getNumDevices;
        private static PalGetDeviceInfoDelegate _getDeviceInfo;
        private static PalSelectDeviceDelegate _selectDevice;
        private static PalReadOpDelegate _readOp;
        private static PalWriteUIntDelegate _writeUInt;
        private static PalWriteUIntFloatDelegate _writeUIntFloat;
        private static PalWriteFloatUIntDelegate _writeFloatUInt;
        private static PalSetBufferSizeDelegate _setBufferSize;
        private static bool _connected;
        private static bool _running;
        private static volatile bool _mox;
        private static double _rxMHz = 14.074;
        private static double _txMHz = 14.074;
        private static uint _firmware;
        private static uint _serial;
        private static int _rxStream = -1;
        private static int _txStream = -1;
        private static int _rxBlock;
        private static int _txBlock;
        private static int _rxFill;
        private static int _txFill;
        private static int _txDecimatePhase;
        private static IntPtr _rxCmBuffer;
        private static IntPtr _txCmBuffer;
        private static FlexAsioCallback _asioCallback;
        private static CmOutboundDelegate _rxAudioCallback;
        private static CmOutboundDelegate _txIqCallback;

        private const int AudioRingSize = 16384;
        private const int AudioRingMask = AudioRingSize - 1;
        private static readonly float[] AudioL = new float[AudioRingSize];
        private static readonly float[] AudioR = new float[AudioRingSize];
        private static int _audioRead;
        private static int _audioWrite;
        private static int _audioRepeat;
        private static float _heldL;
        private static float _heldR;

        private const int TxRingSize = 65536;
        private const int TxRingMask = TxRingSize - 1;
        private static readonly float[] TxI = new float[TxRingSize];
        private static readonly float[] TxQ = new float[TxRingSize];
        private static int _txRead;
        private static int _txWrite;

        internal static bool Connected { get { return _connected; } }
        internal static bool Running { get { return _running; } }
        internal static uint Firmware { get { return _firmware; } }
        internal static uint Serial { get { return _serial; } }

        internal static int InitializeRadio()
        {
            lock (Sync)
            {
                if (_connected) return 0;
                if (IntPtr.Size != 4)
                {
                    Log("INIT_FAIL|REASON=NOT_X86");
                    return -5001;
                }
                try
                {
                    string palPath = Path.Combine(FlexRuntimeDir, PalFileName);
                    _palModule = File.Exists(palPath)
                        ? LoadLibraryEx(palPath, IntPtr.Zero, LoadWithAlteredSearchPath)
                        : IntPtr.Zero;
                    if (_palModule == IntPtr.Zero) _palModule = LoadLibrary(PalFileName);
                    if (_palModule == IntPtr.Zero) throw new InvalidOperationException("pal.dll not found. Install PowerSDR v2.8.0 FLEX runtime.");

                    _palInit = Bind<PalInitDelegate>("Init");
                    _getNumDevices = Bind<PalGetNumDevicesDelegate>("GetNumDevices");
                    _getDeviceInfo = Bind<PalGetDeviceInfoDelegate>("GetDeviceInfo");
                    _selectDevice = Bind<PalSelectDeviceDelegate>("SelectDevice");
                    _readOp = Bind<PalReadOpDelegate>("ReadOp");
                    _writeUInt = Bind<PalWriteUIntDelegate>("WriteOp");
                    _writeUIntFloat = Bind<PalWriteUIntFloatDelegate>("WriteOp");
                    _writeFloatUInt = Bind<PalWriteFloatUIntDelegate>("WriteOp");
                    _setBufferSize = Bind<PalSetBufferSizeDelegate>("SetBufferSize");

                    if (!_palInit()) throw new InvalidOperationException("PAL Init failed");
                    int count = _getNumDevices();
                    if (count < 1) throw new InvalidOperationException("No FLEX-5000 found by PAL");
                    uint model;
                    if (!_getDeviceInfo(0, out model, out _serial)) throw new InvalidOperationException("PAL GetDeviceInfo failed");
                    if (!_selectDevice(0)) throw new InvalidOperationException("PAL SelectDevice failed");
                    _setBufferSize(1024);
                    int initRc = _writeUInt(OpInitialize, 0, 0);
                    _writeUInt(OpSetTrxPreamp, 0, 0);
                    uint fw;
                    int fwRc = _readOp(OpGetFirmwareRev, 0, 0, out fw);
                    if (fwRc != 0) _firmware = fw;
                    SafeTxOff();
                    _writeUInt(OpSetStandby, 0, 0);
                    _connected = true;
                    Log("INIT_PASS|DEVICES=" + count + "|MODEL=" + model + "|SERIAL=" + _serial + "|FW=0x" + _firmware.ToString("X8") + "|INIT_RC=" + initRc);
                    return 0;
                }
                catch (Exception ex)
                {
                    Log("INIT_FAIL|" + ex.GetType().Name + "|" + ex.Message);
                    _connected = false;
                    return -5002;
                }
            }
        }

        internal static void AttachChannelMaster()
        {
            if (_rxAudioCallback == null) _rxAudioCallback = new CmOutboundDelegate(CmAudioOut);
            if (_txIqCallback == null) _txIqCallback = new CmOutboundDelegate(CmTxOut);
            SendpOutboundRx(_rxAudioCallback);
            SendpOutboundTx(_txIqCallback);
            Log("CHANNELMASTER_CALLBACKS=ATTACHED");
        }

        internal static int Start()
        {
            lock (Sync)
            {
                if (_running) return 0;
                if (!_connected)
                {
                    int rc = InitializeRadio();
                    if (rc != 0) return rc;
                }
                try
                {
                    _rxStream = cmaster.inid(0, 0);
                    _txStream = cmaster.inid(1, 0);
                    cmaster.SetXcmInrate(_rxStream, SampleRate);
                    cmaster.SetXcmInrate(_txStream, TxInputRate);
                    cmaster.SetXmtrChannelOutrate(0, SampleRate, cmaster.MONMixState);
                    _rxBlock = cmaster.GetBuffSize(SampleRate);
                    _txBlock = cmaster.GetBuffSize(TxInputRate);
                    if (_rxBlock <= 0 || _rxBlock > 8192 || _txBlock <= 0 || _txBlock > 8192)
                        throw new InvalidOperationException("Invalid ChannelMaster block sizes RX=" + _rxBlock + " TX=" + _txBlock);
                    FreeBuffers();
                    _rxCmBuffer = Marshal.AllocHGlobal(_rxBlock * 2 * sizeof(double));
                    _txCmBuffer = Marshal.AllocHGlobal(_txBlock * 2 * sizeof(double));
                    _rxFill = _txFill = _txDecimatePhase = 0;
                    ResetRings();
                    AttachChannelMaster();

                    int audioId = cmaster.chid(_rxStream, 0);
                    cmaster.SetAAudioMixWhat((void*)0, 0, audioId, !Audio.MuteRX1);
                    cmaster.SetAAudioMixState((void*)0, 0, audioId, true);

                    ConfigureRxAudio();
                    ConfigureTxMic();
                    TuneRx(_rxMHz);

                    if (_asioCallback == null) _asioCallback = new FlexAsioCallback(AsioCallback);
                    int prep = PrepareFlexAsio(SampleRate, FlexAsioDriver, _asioCallback);
                    if (prep != 0) throw new InvalidOperationException("prepareFlexASIO failed rc=" + prep + " err=" + GetFlexAsioError());
                    int block = GetFlexAsioBlockSize();
                    if (block <= 0 || block > 16384) throw new InvalidOperationException("Invalid FLEX ASIO block=" + block);
                    _running = true;
                    int startRc = FlexAsioStart();
                    if (startRc != 0)
                    {
                        _running = false;
                        throw new InvalidOperationException("flexAsioStart failed rc=" + startRc);
                    }
                    Log("START_PASS|ASIO=" + FlexAsioDriver + "|RATE=" + SampleRate + "|BLOCK=" + block + "|RX_CM=" + _rxBlock + "|TX_CM=" + _txBlock);
                    return 0;
                }
                catch (Exception ex)
                {
                    _running = false;
                    try { FlexAsioStop(); } catch { }
                    try { UnloadFlexAsio(); } catch { }
                    SafeTxOff();
                    Log("START_FAIL|" + ex.GetType().Name + "|" + ex.Message);
                    return -5003;
                }
            }
        }

        internal static int Stop()
        {
            lock (Sync)
            {
                try
                {
                    SetMox(false);
                    _running = false;
                    try { FlexAsioStop(); } catch { }
                    try { UnloadFlexAsio(); } catch { }
                    MuteRxAudio();
                    FreeBuffers();
                    ResetRings();
                    Log("STOP_PASS|PAL_EXIT=SKIPPED_PROVEN_LEGACY_HANG");
                    return 0;
                }
                catch (Exception ex)
                {
                    Log("STOP_FAIL|" + ex.GetType().Name + "|" + ex.Message);
                    return -5004;
                }
            }
        }

        internal static void SetFrequency(int id, double mhz, bool tx)
        {
            if (Double.IsNaN(mhz) || Double.IsInfinity(mhz) || mhz <= 0.0 || mhz > 65.0) return;
            if (tx)
            {
                _txMHz = mhz;
                if (_connected) TuneTx(mhz);
            }
            else if (id == 0)
            {
                _rxMHz = mhz;
                if (_connected) TuneRx(mhz);
            }
        }

        internal static bool SetMox(bool on)
        {
            lock (Sync)
            {
                if (!_connected || !_running) return false;
                try
                {
                    if (on)
                    {
                        if (_txMHz < 1.8 || _txMHz > 30.0)
                        {
                            Log("MOX_BLOCKED|FREQ_MHZ=" + _txMHz.ToString("F6"));
                            return false;
                        }
                        TuneTx(_txMHz);
                        ResetTxRing();
                        int antRc = _writeUInt(OpSetTxAnt, 1, 0);
                        int qseRc = _writeUInt(OpSetQse, 1, 0);
                        int trRc = _writeUInt(OpSetTr, 1, 0);
                        int biasRc = _writeUInt(OpSetPaBias, 1, 0);
                        int moxRc = _writeUInt(OpSetMox, 1, 0);
                        _mox = true;
                        Log("MOX_ON|RC=" + moxRc + "|TXANT_RC=" + antRc + "|QSE_RC=" + qseRc + "|TR_RC=" + trRc + "|PA_BIAS_RC=" + biasRc + "|SCALE=" + TxSafetyScale.ToString("F4"));
                        return moxRc == 0 || moxRc == 1;
                    }
                    _mox = false;
                    ResetTxRing();
                    int offRc = _writeUInt(OpSetMox, 0, 0);
                    _writeUInt(OpSetPaBias, 0, 0);
                    _writeUInt(OpSetTr, 0, 0);
                    _writeUInt(OpSetQse, 0, 0);
                    Log("MOX_OFF|RC=" + offRc);
                    return offRc == 0 || offRc == 1;
                }
                catch (Exception ex)
                {
                    _mox = false;
                    SafeTxOff();
                    Log("MOX_FAIL|" + ex.GetType().Name + "|" + ex.Message);
                    return false;
                }
            }
        }

        private static void TuneRx(double mhz)
        {
            uint tw = TuningWord(mhz);
            int frc = _writeFloatUInt(OpSetRx1Filter, (float)mhz, 0);
            int rc = _writeUIntFloat(OpSetRx1FreqTw, tw, (float)mhz);
            Log("RX1_TUNE=PASS|MHZ=" + mhz.ToString("F6") + "|TW=" + tw + "|FILTER_RC=" + frc + "|FREQ_RC=" + rc);
        }

        private static void TuneTx(double mhz)
        {
            uint tw = TuningWord(mhz);
            int trc = _writeFloatUInt(OpSetTxFilter, (float)mhz, 0);
            int prc = _writeFloatUInt(OpSetPaFilter, (float)mhz, 0);
            int rc = _writeUIntFloat(OpSetTxFreqTw, tw, (float)mhz);
            Log("TX_TUNE=PASS|MHZ=" + mhz.ToString("F6") + "|TW=" + tw + "|TRX_FILTER_RC=" + trc + "|PA_FILTER_RC=" + prc + "|FREQ_RC=" + rc);
        }

        private static uint TuningWord(double mhz)
        {
            return (uint)(4294967295.0 * mhz / 500.0);
        }

        private static void ConfigureRxAudio()
        {
            _writeUInt(OpSetRx1Ant, 1, 0);
            _writeUInt(OpSetStandby, 0, 0);
            _writeUInt(OpSetHeadphone, 1, 0);
            _writeUInt(OpSetIntSpkr, 1, 0);
            _writeUInt(OpWriteCodecReg, 0x07, 0x00);
            _writeUInt(OpWriteCodecReg, 0x0A, 0x07);
            _writeUInt(OpWriteCodecReg, 0x0B, 0x07);
            _writeUInt(OpWriteCodecReg, 0x0C, 0x07);
            _writeUInt(OpWriteCodecReg, 0x0D, 0x07);
            _writeUInt(OpWriteCodecReg, 0x0E, 0x07);
            _writeUInt(OpWriteCodecReg, 0x0F, 0x19);
        }

        private static void ConfigureTxMic()
        {
            _writeUInt(OpWriteCodecReg, 0x13, 0x80);
            _writeUInt(OpWriteCodecReg, 0x14, 0x80);
            _writeUInt(OpWriteCodecReg, 0x15, 0x80);
            _writeUInt(OpWriteCodecReg, 0x16, 0xDF);
        }

        private static void MuteRxAudio()
        {
            if (_writeUInt == null) return;
            try { _writeUInt(OpWriteCodecReg, 0x07, 0xFC); } catch { }
            try { _writeUInt(OpSetHeadphone, 0, 0); } catch { }
            try { _writeUInt(OpSetIntSpkr, 0, 0); } catch { }
        }

        private static void SafeTxOff()
        {
            _mox = false;
            ResetTxRing();
            if (_writeUInt == null) return;
            try { _writeUInt(OpSetMox, 0, 0); } catch { }
            try { _writeUInt(OpSetPaBias, 0, 0); } catch { }
            try { _writeUInt(OpSetTr, 0, 0); } catch { }
            try { _writeUInt(OpSetQse, 0, 0); } catch { }
        }

        private static void AsioCallback(
            IntPtr in0, IntPtr in1, IntPtr in2, IntPtr in3,
            IntPtr in4, IntPtr in5, IntPtr in6, IntPtr in7,
            IntPtr out0, IntPtr out1, IntPtr out2, IntPtr out3,
            IntPtr out4, IntPtr out5, IntPtr out6, IntPtr out7,
            int frames)
        {
            try
            {
                if (!_running || frames <= 0 || frames > 16384 || in0 == IntPtr.Zero || in1 == IntPtr.Zero) return;
                float* iSrc = (float*)in0.ToPointer();
                float* qSrc = (float*)in1.ToPointer();
                double* rx = (double*)_rxCmBuffer.ToPointer();
                for (int n = 0; n < frames; n++)
                {
                    int p = _rxFill;
                    rx[2 * p] = iSrc[n];
                    rx[2 * p + 1] = qSrc[n];
                    p++;
                    if (p >= _rxBlock)
                    {
                        cmaster.Inbound(_rxStream, _rxBlock, rx);
                        p = 0;
                    }
                    _rxFill = p;
                }

                if (in7 != IntPtr.Zero && _txCmBuffer != IntPtr.Zero)
                {
                    float* mic = (float*)in7.ToPointer();
                    double* tx = (double*)_txCmBuffer.ToPointer();
                    for (int n = 0; n < frames; n++)
                    {
                        if (_txDecimatePhase == 0)
                        {
                            int p = _txFill;
                            double s = mic[n];
                            tx[2 * p] = s;
                            tx[2 * p + 1] = s;
                            p++;
                            if (p >= _txBlock)
                            {
                                cmaster.Inbound(_txStream, _txBlock, tx);
                                p = 0;
                            }
                            _txFill = p;
                        }
                        _txDecimatePhase = (_txDecimatePhase + 1) & 3;
                    }
                }

                RenderAudio((float*)out2.ToPointer(), (float*)out3.ToPointer(), (float*)out4.ToPointer(), (float*)out5.ToPointer(), (float*)out6.ToPointer(), (float*)out7.ToPointer(), frames);
                if (_mox && out0 != IntPtr.Zero && out1 != IntPtr.Zero)
                    RenderTx((float*)out0.ToPointer(), (float*)out1.ToPointer(), frames);
            }
            catch (Exception ex)
            {
                Log("ASIO_CALLBACK_FAIL|" + ex.GetType().Name + "|" + ex.Message);
            }
        }

        private static void CmAudioOut(int id, int nsamples, double* buffer)
        {
            if (!_running || buffer == null || nsamples <= 0) return;
            for (int n = 0; n < nsamples; n++)
            {
                int w = Volatile.Read(ref _audioWrite);
                int next = (w + 1) & AudioRingMask;
                if (next == Volatile.Read(ref _audioRead)) break;
                AudioL[w] = Clip((float)buffer[2 * n]);
                AudioR[w] = Clip((float)buffer[2 * n + 1]);
                Volatile.Write(ref _audioWrite, next);
            }
        }

        private static void CmTxOut(int id, int nsamples, double* buffer)
        {
            if (!_running || buffer == null || nsamples <= 0) return;
            for (int n = 0; n < nsamples; n++)
            {
                int w = Volatile.Read(ref _txWrite);
                int next = (w + 1) & TxRingMask;
                if (next == Volatile.Read(ref _txRead)) break;
                TxI[w] = Clip((float)buffer[2 * n]);
                TxQ[w] = Clip((float)buffer[2 * n + 1]);
                Volatile.Write(ref _txWrite, next);
            }
        }

        private static void RenderAudio(float* hpR, float* hpL, float* extR, float* extL, float* line, float* speaker, int frames)
        {
            for (int n = 0; n < frames; n++)
            {
                if (_audioRepeat == 0)
                {
                    int r = Volatile.Read(ref _audioRead);
                    if (r != Volatile.Read(ref _audioWrite))
                    {
                        _heldL = AudioL[r];
                        _heldR = AudioR[r];
                        Volatile.Write(ref _audioRead, (r + 1) & AudioRingMask);
                    }
                    else
                    {
                        _heldL = _heldR = 0.0f;
                    }
                }
                if (hpR != null) hpR[n] = _heldR;
                if (hpL != null) hpL[n] = _heldL;
                if (extR != null) extR[n] = _heldR;
                if (extL != null) extL[n] = _heldL;
                if (line != null) line[n] = _heldL;
                if (speaker != null) speaker[n] = _heldL;
                _audioRepeat = (_audioRepeat + 1) & 3;
            }
        }

        private static void RenderTx(float* iDst, float* qDst, int frames)
        {
            for (int n = 0; n < frames; n++)
            {
                int r = Volatile.Read(ref _txRead);
                if (r == Volatile.Read(ref _txWrite))
                {
                    iDst[n] = qDst[n] = 0.0f;
                    continue;
                }
                iDst[n] = TxSafetyScale * TxI[r];
                qDst[n] = TxSafetyScale * TxQ[r];
                Volatile.Write(ref _txRead, (r + 1) & TxRingMask);
            }
        }

        private static void ResetRings()
        {
            Volatile.Write(ref _audioRead, 0);
            Volatile.Write(ref _audioWrite, 0);
            _audioRepeat = 0;
            _heldL = _heldR = 0.0f;
            ResetTxRing();
        }

        private static void ResetTxRing()
        {
            Volatile.Write(ref _txRead, 0);
            Volatile.Write(ref _txWrite, 0);
        }

        private static float Clip(float v)
        {
            if (v > 1.0f) return 1.0f;
            if (v < -1.0f) return -1.0f;
            return v;
        }

        private static void FreeBuffers()
        {
            if (_rxCmBuffer != IntPtr.Zero) { Marshal.FreeHGlobal(_rxCmBuffer); _rxCmBuffer = IntPtr.Zero; }
            if (_txCmBuffer != IntPtr.Zero) { Marshal.FreeHGlobal(_txCmBuffer); _txCmBuffer = IntPtr.Zero; }
        }

        private static T Bind<T>(string name) where T : class
        {
            IntPtr p = GetProcAddress(_palModule, name);
            if (p == IntPtr.Zero) throw new MissingMethodException("PAL export missing: " + name);
            return Marshal.GetDelegateForFunctionPointer(p, typeof(T)) as T;
        }

        private static void Log(string text)
        {
            try
            {
                File.AppendAllText(Path.Combine(Application.StartupPath, "FLEX5000_RUNTIME.log"), DateTime.Now.ToString("HH:mm:ss.fff") + " " + text + "\r\n");
            }
            catch { }
        }
    }
}
