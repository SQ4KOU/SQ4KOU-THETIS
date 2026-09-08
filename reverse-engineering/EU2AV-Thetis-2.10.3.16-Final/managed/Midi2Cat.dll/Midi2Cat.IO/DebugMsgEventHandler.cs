using Midi2Cat.Data;

namespace Midi2Cat.IO;

public delegate void DebugMsgEventHandler(int Device, Direction direction, Status deviceStatus, string msg1, string msg2);
