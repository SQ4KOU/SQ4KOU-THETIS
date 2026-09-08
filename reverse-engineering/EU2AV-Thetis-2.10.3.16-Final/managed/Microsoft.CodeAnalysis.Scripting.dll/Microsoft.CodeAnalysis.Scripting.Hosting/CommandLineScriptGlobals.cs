using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Scripting.Hosting;

public class CommandLineScriptGlobals
{
	private readonly TextWriter _outputWriter;

	private readonly ObjectFormatter _objectFormatter;

	public IList<string> Args { get; }

	public PrintOptions PrintOptions { get; }

	public void Print(object value)
	{
		_outputWriter.WriteLine(_objectFormatter.FormatObject(value, PrintOptions));
	}

	public CommandLineScriptGlobals(TextWriter outputWriter, ObjectFormatter objectFormatter)
	{
		if (outputWriter == null)
		{
			throw new ArgumentNullException("outputWriter");
		}
		if (objectFormatter == null)
		{
			throw new ArgumentNullException("objectFormatter");
		}
		PrintOptions = new PrintOptions();
		_outputWriter = outputWriter;
		_objectFormatter = objectFormatter;
		Args = new List<string>();
	}
}
