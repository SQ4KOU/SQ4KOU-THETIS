namespace Thetis;

public class DigiMode
{
	public enum DigiModeSettingState
	{
		dmssTurnOffSettings,
		dmssStore,
		dmssRecall
	}

	public bool DEXP { get; set; }

	public bool TXEQ { get; set; }

	public bool LEVELER { get; set; }

	public bool COMPRESSOR { get; set; }

	public bool RXEQ { get; set; }

	public bool ANF { get; set; }

	public bool CESSB { get; set; }

	public int NR { get; set; }

	public bool CFCEnabled { get; set; }

	public bool PhaseRotEnabled { get; set; }

	public DigiModeSettingState Mode { get; set; }
}
