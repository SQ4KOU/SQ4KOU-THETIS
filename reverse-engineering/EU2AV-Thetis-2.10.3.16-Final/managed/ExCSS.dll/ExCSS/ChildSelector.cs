using System.IO;

namespace ExCSS;

public abstract class ChildSelector : StylesheetNode, ISelector, IStylesheetNode, IStyleFormattable
{
	private readonly string _name;

	protected ISelector Kind;

	public int Step { get; private set; }

	public int Offset { get; private set; }

	public Priority Specificity => Priority.OneClass;

	public string Text => this.ToCss();

	protected ChildSelector(string name)
	{
		_name = name;
	}

	public override void ToCss(TextWriter writer, IStyleFormatter formatter)
	{
		string arg = Step.ToString();
		int offset = Offset;
		string text = ((offset > 0) ? ("+" + Offset) : ((offset >= 0) ? string.Empty : Offset.ToString()));
		string arg2 = text;
		writer.Write(":{0}({1}n{2})", _name, arg, arg2);
	}

	internal ChildSelector With(int step, int offset, ISelector kind)
	{
		Step = step;
		Offset = offset;
		Kind = kind;
		return this;
	}
}
