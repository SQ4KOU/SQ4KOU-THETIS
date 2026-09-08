using System;
using System.IO;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

internal class ConsoleIO
{
	public static readonly ConsoleIO Default = new ConsoleIO(Console.Out, Console.Error, Console.In);

	public TextWriter Error { get; }

	public TextWriter Out { get; }

	public TextReader In { get; }

	public ConsoleIO(TextWriter output, TextWriter error, TextReader input)
	{
		Out = output;
		Error = error;
		In = input;
	}

	public virtual void SetForegroundColor(ConsoleColor consoleColor)
	{
		Console.ForegroundColor = consoleColor;
	}

	public virtual void ResetColor()
	{
		Console.ResetColor();
	}
}
