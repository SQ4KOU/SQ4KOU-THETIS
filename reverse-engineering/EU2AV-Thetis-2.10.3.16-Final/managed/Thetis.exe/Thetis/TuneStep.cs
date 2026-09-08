namespace Thetis;

public class TuneStep
{
	private int step_hz;

	private string name;

	public int StepHz => step_hz;

	public string Name => name;

	public TuneStep(int _step_hz, string _name)
	{
		step_hz = _step_hz;
		name = _name;
	}

	public override string ToString()
	{
		return name;
	}
}
