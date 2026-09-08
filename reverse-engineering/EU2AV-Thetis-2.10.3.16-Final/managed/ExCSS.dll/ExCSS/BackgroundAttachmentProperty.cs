namespace ExCSS;

internal sealed class BackgroundAttachmentProperty : Property
{
	private static readonly IValueConverter AttachmentConverter = Converters.BackgroundAttachmentConverter.FromList().OrDefault(BackgroundAttachment.Scroll);

	internal override IValueConverter Converter => AttachmentConverter;

	internal BackgroundAttachmentProperty()
		: base(PropertyNames.BackgroundAttachment)
	{
	}
}
