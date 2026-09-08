using System.Collections.Generic;

namespace Discord;

public class ModifyApplicationProperties
{
	public Optional<string> InteractionsEndpointUrl { get; set; }

	public Optional<string> RoleConnectionsEndpointUrl { get; set; }

	public Optional<string> Description { get; set; }

	public Optional<string[]> Tags { get; set; }

	public Optional<Image?> Icon { get; set; }

	public Optional<Image?> CoverImage { get; set; }

	public Optional<string> CustomInstallUrl { get; set; }

	public Optional<ApplicationInstallParams> InstallParams { get; set; }

	public Optional<ApplicationFlags> Flags { get; set; }

	public Optional<Dictionary<ApplicationIntegrationType, ApplicationInstallParams>> IntegrationTypesConfig { get; set; }
}
