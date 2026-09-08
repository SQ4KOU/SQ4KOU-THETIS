namespace SharpDX.DirectWrite;

public enum FontPropertyId
{
	None = 0,
	WeightStretchStyleFamilyName = 1,
	TypographicFamilyName = 2,
	WeightStretchStyleFaceName = 3,
	FullName = 4,
	Win32FamilyName = 5,
	PostscriptName = 6,
	DesignScriptLanguageTag = 7,
	SupportedScriptLanguageTag = 8,
	SemanticTag = 9,
	Weight = 10,
	Stretch = 11,
	Style = 12,
	TypographicFaceName = 13,
	Total = TypographicFaceName,
	TotalRasterizer3 = 14,
	PreferRedFamilyName = TypographicFamilyName,
	FamilyName = WeightStretchStyleFamilyName,
	FaceName = WeightStretchStyleFaceName
}
