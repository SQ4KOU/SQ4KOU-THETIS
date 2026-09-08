using Midi2Cat.IO;

namespace Midi2Cat.Data;

public delegate CmdState ProcessMidiMessageToggleHandler(int msg, MidiDevice device);
