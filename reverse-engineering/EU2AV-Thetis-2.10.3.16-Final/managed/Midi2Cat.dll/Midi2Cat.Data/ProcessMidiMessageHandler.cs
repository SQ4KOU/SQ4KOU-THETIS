using Midi2Cat.IO;

namespace Midi2Cat.Data;

public delegate void ProcessMidiMessageHandler(int msg, MidiDevice device);
